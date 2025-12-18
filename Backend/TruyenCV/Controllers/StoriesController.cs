using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruyenCV.Dtos.Stories;
using TruyenCV.Services;

namespace TruyenCV.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _service;
    public StoriesController(IStoryService service) => _service = service;

    // GET: api/stories/all?authorId=1&primaryGenreId=2&q=abc
    [HttpGet("all")]
    public async Task<IActionResult> GetAll([FromQuery] int? authorId = null, [FromQuery] int? primaryGenreId = null, [FromQuery] string? q = null)
    {
        var data = await _service.GetAllAsync(authorId, primaryGenreId, q);
        return Ok(new { status = true, message = "Lấy danh sách truyện thành công.", data });
    }

    // GET: api/stories/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto is null
            ? NotFound(new { status = false, message = "Không tìm thấy truyện.", data = (object?)null })
            : Ok(new { status = true, message = "Lấy thông tin truyện thành công.", data = dto });
    }

    // POST: api/stories/create
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] StoryCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
            {
                status = false,
                message = "Dữ liệu không hợp lệ.",
                data = (object?)null,
                errors = ToErrorDict(ModelState)
            });

        try
        {
            var created = await _service.CreateAsync(dto);
            return StatusCode(201, new { status = true, message = "Tạo truyện thành công.", data = created });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    // PUT: api/stories/update-5
    [HttpPut("update-{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] StoryUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new
            {
                status = false,
                message = "Dữ liệu không hợp lệ.",
                data = (object?)null,
                errors = ToErrorDict(ModelState)
            });

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(new { status = true, message = "Cập nhật truyện thành công.", data = updated });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    // DELETE: api/stories/delete-5
    [HttpDelete("delete-{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok
            ? Ok(new { status = true, message = "Xóa truyện thành công.", data = new { storyId = id } })
            : NotFound(new { status = false, message = "Không tìm thấy truyện để xóa.", data = (object?)null });
    }

    private static Dictionary<string, string[]> ToErrorDict(ModelStateDictionary modelState)
        => modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Dữ liệu không hợp lệ." : e.ErrorMessage)
                    .ToArray()
            );
}
