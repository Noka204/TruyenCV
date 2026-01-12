using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruyenCV.Dtos.Authors;
using TruyenCV.Services;

namespace TruyenCV.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorService _service;
    public AuthorsController(IAuthorService service) => _service = service;

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => Ok(new { status = true, message = "Lấy danh sách tác giả thành công.", data = await _service.GetAllAsync() });

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        return dto is null
            ? NotFound(new { status = false, message = "Không tìm thấy tác giả.", data = (object?)null })
            : Ok(new { status = true, message = "Lấy thông tin tác giả thành công.", data = dto });
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] AuthorCreateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { status = false, message = "Dữ liệu không hợp lệ.", data = (object?)null, errors = ToErrorDict(ModelState) });

        try
        {
            var newId = await _service.CreateAsync(dto);
            return StatusCode(201, new { status = true, message = "Tạo tác giả thành công.", data = new { authorId = newId } });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPut("update-{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] AuthorUpdateDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { status = false, message = "Dữ liệu không hợp lệ.", data = (object?)null, errors = ToErrorDict(ModelState) });

        try
        {
            var ok = await _service.UpdateAsync(id, dto);
            return ok
                ? Ok(new { status = true, message = "Cập nhật tác giả thành công.", data = new { authorId = id } })
                : NotFound(new { status = false, message = "Không tìm thấy tác giả để cập nhật.", data = (object?)null });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok
            ? Ok(new { status = true, message = "Xóa tác giả thành công.", data = new { authorId = id } })
            : NotFound(new { status = false, message = "Không tìm thấy tác giả để xóa.", data = (object?)null });
    }

    // ===== Author Approval Workflow =====

    [Authorize]
    [HttpPost("request")]
    public async Task<IActionResult> SubmitRequest([FromBody] AuthorRequestDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { status = false, message = "Dữ liệu không hợp lệ.", data = (object?)null, errors = ToErrorDict(ModelState) });

        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { status = false, message = "Không tìm thấy thông tin user.", data = (object?)null });

            var authorId = await _service.SubmitAuthorRequestAsync(userId, dto);
            return StatusCode(201, new { status = true, message = "Gửi yêu cầu đăng ký làm tác giả thành công.", data = new { authorId } });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    [Authorize]
    [HttpGet("my-status")]
    public async Task<IActionResult> GetMyStatus()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { status = false, message = "Không tìm thấy thông tin user.", data = (object?)null });

        var status = await _service.GetMyAuthorStatusAsync(userId);
        return status is null
            ? NotFound(new { status = false, message = "Bạn chưa gửi yêu cầu đăng ký làm tác giả.", data = (object?)null })
            : Ok(new { status = true, message = "Lấy trạng thái thành công.", data = status });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingAuthors()
    {
        var data = await _service.GetPendingAuthorsAsync();
        return Ok(new { status = true, message = "Lấy danh sách tác giả chờ duyệt thành công.", data });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> ApproveAuthor(int id)
    {
        try
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminId))
                return Unauthorized(new { status = false, message = "Không tìm thấy thông tin admin.", data = (object?)null });

            var ok = await _service.ApproveAuthorAsync(id, adminId);
            return ok
                ? Ok(new { status = true, message = "Duyệt tác giả thành công.", data = new { authorId = id } })
                : NotFound(new { status = false, message = "Không tìm thấy tác giả.", data = (object?)null });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}/reject")]
    public async Task<IActionResult> RejectAuthor(int id)
    {
        var ok = await _service.RejectAuthorAsync(id);
        return ok
            ? Ok(new { status = true, message = "Đã từ chối và xóa yêu cầu đăng ký.", data = new { authorId = id } })
            : NotFound(new { status = false, message = "Không tìm thấy tác giả.", data = (object?)null });
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
