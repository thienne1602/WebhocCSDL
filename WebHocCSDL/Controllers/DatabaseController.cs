using Microsoft.AspNetCore.Mvc;
using WebHocCSDL.Data;
using WebHocCSDL.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace WebHocCSDL.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly AppDbContext _context;

        public DatabaseController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(DatabaseRequest? request)
        {
            if (!ModelState.IsValid || request?.RequestText == null)
            {
                return View(new DatabaseRequest());
            }

            var design = AutoAnalyzeRequest(request.RequestText);
            Console.WriteLine($"Analyzed Design: Entities={string.Join(", ", design.Entities?.Select(e => e.Name ?? "null") ?? new string[] { "null" })}, Relationships={string.Join(", ", design.Relationships?.Select(r => $"{r.Entity1 ?? "null"}-{r.Entity2 ?? "null"} ({r.Type ?? "null"})") ?? new string[] { "null" })}");
            return RedirectToAction("EditEntities", new { design = Newtonsoft.Json.JsonConvert.SerializeObject(design) });
        }

        public IActionResult EditEntities(string design)
        {
            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseDesign>(design) ?? new DatabaseDesign
            {
                RequirementDescription = "Không phân tích được yêu cầu",
                Entities = new List<Entity>(),
                Relationships = new List<Relationship>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEntities(DatabaseDesign design)
        {
            try
            {
                design.RequirementDescription ??= "Yêu cầu mặc định";
                design.Entities = design.Entities?.Where(e => !string.IsNullOrEmpty(e.Name)).ToList() ?? new List<Entity>();
                foreach (var entity in design.Entities)
                {
                    // Chuyển đổi chuỗi thuộc tính thành danh sách
                    if (entity.Attributes != null && entity.Attributes.Count == 1 && entity.Attributes[0]?.Contains(",") == true)
                    {
                        entity.Attributes = entity.Attributes[0].Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(attr => attr.Trim())
                            .Where(attr => !string.IsNullOrEmpty(attr))
                            .ToList();
                    }
                    entity.Attributes = entity.Attributes?.Where(attr => !string.IsNullOrEmpty(attr)).ToList() ?? new List<string>();
                    if (!entity.Attributes.Any(attr => attr.ToLower().StartsWith("mã") || attr.ToLower().Contains("id")))
                    {
                        entity.Attributes.Insert(0, $"Mã {entity.Name?.Replace(" ", "")}");
                    }
                }

                design.Relationships = design.Relationships?.Where(r => !string.IsNullOrEmpty(r.Entity1) && !string.IsNullOrEmpty(r.Entity2) && !string.IsNullOrEmpty(r.Type)).ToList() ?? new List<Relationship>();

                design.ERD = GenerateERD(design.Entities, design.Relationships);
                Console.WriteLine("Generated ERD:\n" + design.ERD);
                design.LogicalDesign = GenerateLogicalDesign(design.Entities, design.Relationships);
                design.PhysicalDesign = GeneratePhysicalDesign(design.Entities, design.Relationships);
                design.ConceptualDesign ??= design.ERD;

                if (string.IsNullOrEmpty(design.ERD) || string.IsNullOrEmpty(design.LogicalDesign) || string.IsNullOrEmpty(design.PhysicalDesign))
                {
                    return BadRequest("Không thể tạo thiết kế CSDL. Vui lòng kiểm tra dữ liệu đầu vào.");
                }

                _context.Designs.Add(design);
                await _context.SaveChangesAsync();
                return View("Result", design);
            }
            catch (DbUpdateException ex)
            {
                var innerException = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"DbUpdateException: {innerException}");
                return BadRequest($"Lỗi khi lưu vào CSDL: {innerException}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        #region Phương thức hỗ trợ
        private DatabaseDesign AutoAnalyzeRequest(string requestText)
        {
            var design = new DatabaseDesign
            {
                RequirementDescription = $"Yêu cầu: {requestText}",
                Entities = new List<Entity>(),
                Relationships = new List<Relationship>()
            };

            requestText = requestText.Replace("\r\n", " ").Replace("\n", " ").ToLower();
            var sentences = Regex.Split(requestText, @"(?<=[\.!\?])\s+")
                               .Select(s => s.Trim())
                               .Where(s => !string.IsNullOrEmpty(s))
                               .ToList();

            var entityPattern = @"(quản lý|bao gồm|có|chứa|liên quan đến|được xác định bởi|với)\s*(?<entity>[\p{L}\s]+?)(?:\s*(?:các\s*)?(?:thuộc tính|thuộc tính như|là|kèm theo|có|với các|với)\s*(?<attributes>[\p{L}\s,]+))?";
            foreach (var sentence in sentences)
            {
                var match = Regex.Match(sentence, entityPattern, RegexOptions.IgnoreCase);
                if (match.Success && !string.IsNullOrEmpty(match.Groups["entity"].Value))
                {
                    var rawEntityName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(match.Groups["entity"].Value.Trim());
                    var entityName = RemoveDiacritics(rawEntityName).Replace(" ", "_");
                    var attributesText = match.Groups["attributes"].Value.Trim();
                    var attributes = attributesText.Split(new[] { ",", " ", "và" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(attr => attr.Trim())
                        .Where(attr => !string.IsNullOrEmpty(attr) && !attr.ToLower().Contains("mỗi") && !attr.ToLower().Contains("một"))
                        .Select(attr => RemoveDiacritics(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(attr)).Replace(" ", "_"))
                        .Where(attr => !design.Entities.Any(e => e.Name?.ToLower() == attr.ToLower()))
                        .Distinct()
                        .ToList();

                    if (!attributes.Any(attr => attr.ToLower().StartsWith("ma") || attr.ToLower().Contains("id")))
                    {
                        attributes.Insert(0, $"Ma_{entityName}");
                    }

                    if (!string.IsNullOrEmpty(entityName) && !entityName.ToLower().Contains("moi") && !entityName.ToLower().Contains("mot"))
                    {
                        if (!design.Entities.Any(e => e.Name?.ToLower() == entityName.ToLower()))
                        {
                            design.Entities.Add(new Entity { Name = entityName, Attributes = attributes });
                        }
                        else
                        {
                            var existingEntity = design.Entities.FirstOrDefault(e => e.Name?.ToLower() == entityName.ToLower());
                            if (existingEntity != null)
                            {
                                existingEntity.Attributes = existingEntity.Attributes.Union(attributes).Distinct().ToList();
                            }
                        }
                    }
                }
            }

            var relationshipPattern = @"(?<entity1>[\p{L}\s]+)\s*(?:thuộc|có thể|được|liên kết với|có)\s*(?:một|nhiều)?\s*(?<entity2>[\p{L}\s]+)(?:\s*(?:học|mua|thuộc về|liên kết|sử dụng)\s*(?:nhiều|một)?)?(?:\s*\((?:quan hệ\s*)?(?<type>[\d:n-]+)\))?";
            foreach (var sentence in sentences)
            {
                var match = Regex.Match(sentence, relationshipPattern, RegexOptions.IgnoreCase);
                if (match.Success && !string.IsNullOrEmpty(match.Groups["entity1"].Value) && !string.IsNullOrEmpty(match.Groups["entity2"].Value))
                {
                    var rawEntity1 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(match.Groups["entity1"].Value.Trim());
                    var rawEntity2 = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(match.Groups["entity2"].Value.Trim());
                    var entity1 = RemoveDiacritics(rawEntity1).Replace(" ", "_");
                    var entity2 = RemoveDiacritics(rawEntity2).Replace(" ", "_");
                    var type = match.Groups["type"].Value.Trim() == "" ?
                              (sentence.ToLower().Contains("nhiều") || sentence.ToLower().Contains("mua") || sentence.ToLower().Contains("học") || sentence.ToLower().Contains("sử dụng") ? "1:N" : "1:1") :
                              match.Groups["type"].Value.Trim();

                    if (!string.IsNullOrEmpty(entity1) && !design.Entities.Any(e => e.Name?.ToLower() == entity1.ToLower()) && !entity1.ToLower().Contains("he_thong"))
                    {
                        design.Entities.Add(new Entity { Name = entity1, Attributes = new List<string> { $"Ma_{entity1}" } });
                    }
                    if (!string.IsNullOrEmpty(entity2) && !design.Entities.Any(e => e.Name?.ToLower() == entity2.ToLower()) && !entity2.ToLower().Contains("he_thong"))
                    {
                        design.Entities.Add(new Entity { Name = entity2, Attributes = new List<string> { $"Ma_{entity2}" } });
                    }

                    if (!string.IsNullOrEmpty(entity1) && !string.IsNullOrEmpty(entity2) &&
                        design.Entities.Any(e => e.Name?.ToLower() == entity1.ToLower()) &&
                        design.Entities.Any(e => e.Name?.ToLower() == entity2.ToLower()) &&
                        !design.Relationships.Any(r => r.Entity1?.ToLower() == entity1.ToLower() && r.Entity2?.ToLower() == entity2.ToLower()))
                    {
                        design.Relationships.Add(new Relationship { Entity1 = entity1, Entity2 = entity2, Type = type });
                    }
                }
            }

            return design;
        }

        private string GenerateERD(List<Entity>? entities, List<Relationship>? relationships)
        {
            StringBuilder erd = new StringBuilder("erDiagram\n");
            if (entities != null)
            {
                foreach (var entity in entities.Where(e => !string.IsNullOrEmpty(e.Name)))
                {
                    var entityName = RemoveDiacritics(entity.Name).Replace(" ", "_");
                    erd.AppendLine($"    {entityName} {{");
                    var attributes = entity.Attributes ?? new List<string>();
                    if (attributes.Any())
                    {
                        foreach (var attr in attributes)
                        {
                            string attrType = attr.ToLower().Contains("ngay") ? "date" :
                                              attr.ToLower().Contains("gia") ? "decimal" : "string";
                            var attrName = RemoveDiacritics(attr).Replace(" ", "_");
                            erd.AppendLine($"        {attrType} {attrName}");
                        }
                    }
                    else
                    {
                        erd.AppendLine($"        string Default_Attribute");
                    }
                    erd.AppendLine("    }");
                }
            }

            if (relationships != null)
            {
                foreach (var rel in relationships.Where(r => !string.IsNullOrEmpty(r.Entity1) && !string.IsNullOrEmpty(r.Entity2)))
                {
                    var entity1 = RemoveDiacritics(rel.Entity1).Replace(" ", "_");
                    var entity2 = RemoveDiacritics(rel.Entity2).Replace(" ", "_");
                    string symbol = rel.Type switch
                    {
                        "1:N" => "||--o{",
                        "N:N" => "}--|{",
                        "1:1" => "||--||",
                        _ => "||--||"
                    };
                    erd.AppendLine($"    {entity1} {symbol} {entity2} : \"{rel.Type ?? "1:1"}\"");
                }
            }

            return erd.ToString().TrimEnd();
        }

        private string GenerateLogicalDesign(List<Entity>? entities, List<Relationship>? relationships)
        {
            StringBuilder logical = new StringBuilder("Bảng:\n");
            foreach (var entity in entities?.Where(e => !string.IsNullOrEmpty(e.Name)) ?? new List<Entity>())
            {
                logical.AppendLine($"- {entity.Name} ({string.Join(", ", (entity.Attributes ?? new List<string>()).Select(a => a + (a == (entity.Attributes?.FirstOrDefault() ?? "") ? " PK" : "")))})");
            }
            foreach (var rel in relationships?.Where(r => !string.IsNullOrEmpty(r.Entity1) && !string.IsNullOrEmpty(r.Entity2)) ?? new List<Relationship>())
            {
                if (rel.Type == "1:N" || rel.Type == "N:N")
                {
                    logical.AppendLine($"- Thêm khóa ngoại vào {rel.Entity2}: {rel.Entity1.Replace(" ", "_")}ID FK");
                }
                if (rel.Type == "N:N")
                {
                    logical.AppendLine($"- Bảng trung gian {rel.Entity1.Replace(" ", "_")}_{rel.Entity2.Replace(" ", "_")} ({rel.Entity1.Replace(" ", "_")}ID FK, {rel.Entity2.Replace(" ", "_")}ID FK)");
                }
            }
            return logical.ToString();
        }

        private string GeneratePhysicalDesign(List<Entity>? entities, List<Relationship>? relationships)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            foreach (var entity in entities?.Where(e => !string.IsNullOrEmpty(e.Name)) ?? new List<Entity>())
            {
                sqlBuilder.AppendLine($"CREATE TABLE {entity.Name.Replace(" ", "_")} (");
                var attributes = entity.Attributes ?? new List<string>();
                if (attributes.Any())
                {
                    sqlBuilder.AppendLine($"    {attributes[0].Replace(" ", "_")} INT PRIMARY KEY,");
                    for (int i = 1; i < attributes.Count; i++)
                    {
                        string dataType = attributes[i].ToLower().Contains("ngày") ? "DATE" :
                                          attributes[i].ToLower().Contains("giá") ? "DECIMAL(10,2)" : "NVARCHAR(100)";
                        sqlBuilder.AppendLine($"    {attributes[i].Replace(" ", "_")} {dataType},");
                    }
                }

                var foreignKeys = relationships?.Where(r => r.Entity2 == entity.Name && (r.Type == "1:N" || r.Type == "N:N") && !string.IsNullOrEmpty(r.Entity1) && !string.IsNullOrEmpty(r.Entity2)).ToList() ?? new List<Relationship>();
                foreach (var fk in foreignKeys)
                {
                    sqlBuilder.AppendLine($"    {fk.Entity1.Replace(" ", "_")}ID INT,");
                    sqlBuilder.AppendLine($"    FOREIGN KEY ({fk.Entity1.Replace(" ", "_")}ID) REFERENCES {fk.Entity1.Replace(" ", "_")}({fk.Entity1.Replace(" ", "_")}ID),");
                }

                if (sqlBuilder.Length > 13)
                {
                    sqlBuilder.Length -= 3; // Xóa dấu phẩy cuối
                }
                sqlBuilder.AppendLine("\n);");
            }

            foreach (var rel in relationships?.Where(r => r.Type == "N:N" && !string.IsNullOrEmpty(r.Entity1) && !string.IsNullOrEmpty(r.Entity2)) ?? new List<Relationship>())
            {
                sqlBuilder.AppendLine($"CREATE TABLE {rel.Entity1.Replace(" ", "_")}_{rel.Entity2.Replace(" ", "_")} (");
                sqlBuilder.AppendLine($"    {rel.Entity1.Replace(" ", "_")}ID INT,");
                sqlBuilder.AppendLine($"    {rel.Entity2.Replace(" ", "_")}ID INT,");
                sqlBuilder.AppendLine($"    PRIMARY KEY ({rel.Entity1.Replace(" ", "_")}ID, {rel.Entity2.Replace(" ", "_")}ID),");
                sqlBuilder.AppendLine($"    FOREIGN KEY ({rel.Entity1.Replace(" ", "_")}ID) REFERENCES {rel.Entity1.Replace(" ", "_")}({rel.Entity1.Replace(" ", "_")}ID),");
                sqlBuilder.AppendLine($"    FOREIGN KEY ({rel.Entity2.Replace(" ", "_")}ID) REFERENCES {rel.Entity2.Replace(" ", "_")}({rel.Entity2.Replace(" ", "_")}ID)");
                sqlBuilder.AppendLine(");");
            }

            return sqlBuilder.ToString();
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC)
                .Replace("đ", "d")
                .Replace("Đ", "D");
        }
        #endregion
    }
}