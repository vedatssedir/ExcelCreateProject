using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMq.ExcelCreate.Models;
using RabbitMq.ExcelCreate.Services;

namespace RabbitMq.ExcelCreate.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DataContext _context;
        private readonly RabbitMqPublisher _rabbitMqPublisher;
        public ProductsController(DataContext context, RabbitMqPublisher rabbitMqPublisher)
        {
            _context = context;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateProductExcel()
        {
            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";
            UserFile userFile = new()
            {
                UserId = Guid.NewGuid().ToString(),
                FileName = fileName,
                FileStatus = FileStatus.Creating,
                FilePath = string.Empty
            };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();
            _rabbitMqPublisher.Publish(new CreateExcelMessage() { FileId = userFile.Id });
            TempData["StartCreatingExcel"] = true;
            return RedirectToAction(nameof(Files));
        }

        public async Task<IActionResult> Files()
        {
            var userFileList = await _context.UserFiles.ToListAsync();
            return View(userFileList);
        }





    }
}
