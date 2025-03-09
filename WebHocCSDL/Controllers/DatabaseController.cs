using Microsoft.AspNetCore.Mvc;
using WebHocCSDL.Data;
using WebHocCSDL.Models;

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
        public async Task<IActionResult> Create(DatabaseRequest request)
        {
            var design = AnalyzeAndDesign(request.RequestText);
            _context.Designs.Add(design);
            await _context.SaveChangesAsync();
            return View("Result", design);
        }

        private DatabaseDesign AnalyzeAndDesign(string requestText)
        {
            // Phân tích yêu cầu (ví dụ: "Quản lý đơn hàng...")
            var design = new DatabaseDesign
            {
                RequirementDescription = "Yêu cầu: " + requestText,
                ConceptualDesign = "Thực thể: Đơn hàng, Khách hàng, Sản phẩm\n" +
                                  "Quan hệ: Đơn hàng - Khách hàng (1:N), Đơn hàng - Sản phẩm (N:N)",
                ERD = "erDiagram\n" +
                      "    DonHang ||--o{ KhachHang : \"thuộc về\"\n" +
                      "    DonHang }|--|{ SanPham : \"chứa\"\n" +
                      "    DonHang {\n" +
                      "        string MaDonHang\n" +
                      "        date NgayDat\n" +
                      "        string TrangThai\n" +
                      "    }\n" +
                      "    KhachHang {\n" +
                      "        string MaKH\n" +
                      "        string TenKH\n" +
                      "        string DiaChi\n" +
                      "    }\n" +
                      "    SanPham {\n" +
                      "        string MaSP\n" +
                      "        string TenSP\n" +
                      "        decimal Gia\n" +
                      "    }",
                LogicalDesign = "Bảng:\n" +
                                "- DonHang (MaDonHang PK, NgayDat, TrangThai, MaKH FK)\n" +
                                "- KhachHang (MaKH PK, TenKH, DiaChi)\n" +
                                "- DonHang_SanPham (MaDonHang FK, MaSP FK, SoLuong)\n" +
                                "- SanPham (MaSP PK, TenSP, Gia)",
                PhysicalDesign = "CREATE TABLE KhachHang (\n" +
                                 "    MaKH NVARCHAR(10) PRIMARY KEY,\n" +
                                 "    TenKH NVARCHAR(50),\n" +
                                 "    DiaChi NVARCHAR(100)\n" +
                                 ");\n" +
                                 "CREATE TABLE DonHang (\n" +
                                 "    MaDonHang NVARCHAR(10) PRIMARY KEY,\n" +
                                 "    NgayDat DATE,\n" +
                                 "    TrangThai NVARCHAR(20),\n" +
                                 "    MaKH NVARCHAR(10),\n" +
                                 "    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH)\n" +
                                 ");\n" +
                                 "CREATE TABLE SanPham (\n" +
                                 "    MaSP NVARCHAR(10) PRIMARY KEY,\n" +
                                 "    TenSP NVARCHAR(50),\n" +
                                 "    Gia DECIMAL(18,2)\n" +
                                 ");\n" +
                                 "CREATE TABLE DonHang_SanPham (\n" +
                                 "    MaDonHang NVARCHAR(10),\n" +
                                 "    MaSP NVARCHAR(10),\n" +
                                 "    SoLuong INT,\n" +
                                 "    PRIMARY KEY (MaDonHang, MaSP),\n" +
                                 "    FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang),\n" +
                                 "    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)\n" +
                                 ");"
            };

            design.Entities = new List<Entity>
            {
                new Entity { Name = "DonHang", Attributes = new List<string> { "MaDonHang", "NgayDat", "TrangThai", "MaKH" } },
                new Entity { Name = "KhachHang", Attributes = new List<string> { "MaKH", "TenKH", "DiaChi" } },
                new Entity { Name = "SanPham", Attributes = new List<string> { "MaSP", "TenSP", "Gia" } }
            };

            design.Relationships = new List<Relationship>
            {
                new Relationship { Entity1 = "DonHang", Entity2 = "KhachHang", Type = "1:N" },
                new Relationship { Entity1 = "DonHang", Entity2 = "SanPham", Type = "N:N" }
            };

            return design;
        }
    }
}