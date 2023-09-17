using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMq.ExcelCreate.Models;
using FileStream = System.IO.FileStream;

namespace RabbitMq.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly DataContext _dataContext;

        public FilesController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile formFile, int fileId)
        {

            if (formFile is not { Length: > 0 })
            {
                return BadRequest();
            }


            var userFile = await _dataContext.UserFiles.FirstOrDefaultAsync(x => x.Id == fileId);


            var filePath = userFile?.FileName + Path.GetExtension(formFile.FileName);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            await using var stream = new FileStream(path, FileMode.Create);
            await formFile.CopyToAsync(stream);

            userFile!.CreateDate =DateTime.Now;
            userFile!.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;
            await _dataContext.SaveChangesAsync();
            return Ok();
        }






    }
}
