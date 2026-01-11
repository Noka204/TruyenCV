using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using TruyenCV.Dtos.Bookmarks;
using TruyenCV.Services.IService;

namespace TruyenCV.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookmarksController : ControllerBase
{
    private readonly IBookmarkService _service;

    public BookmarksController(IBookmarkService service) => _service = service;

    [HttpGet("my-bookmarks")]
    public async Task<IActionResult> GetMyBookmarks([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { status = false, message = "Ng??i d�ng ch?a x�c th?c.", data = (object?)null });

        try
        {
            if (page < 1)
                return BadRequest(new { status = false, message = "Trang ph?i t? 1 tr? l�n.", data = (object?)null });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { status = false, message = "K�ch th??c trang ph?i t? 1 ??n 100.", data = (object?)null });

            var bookmarks = await _service.GetUserBookmarksAsync(userId, page, pageSize);
            var count = await _service.GetUserBookmarksCountAsync(userId);

            return Ok(new
            {
                status = true,
                message = "L?y danh s�ch truy?n ?� l?u th�nh c�ng.",
                data = new { bookmarks, total = count, page, pageSize }
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { status = false, message = "Kh�ng t�m th?y ng??i d�ng.", data = (object?)null });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, data = (object?)null });
        }
    }


    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] BookmarkCreateDTO dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { status = false, message = "Ng??i d�ng ch?a x�c th?c.", data = (object?)null });

        if (!ModelState.IsValid)
            return BadRequest(new { status = false, message = "D? li?u kh�ng h?p l?.", data = (object?)null, errors = ToErrorDict(ModelState) });

        try
        {
            var created = await _service.CreateAsync(userId, dto);
            return StatusCode(201, new { status = true, message = "L?u truy?n th�nh c�ng.", data = created });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    [HttpDelete("delete/{storyId:int}")]
    public async Task<IActionResult> Delete(int storyId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { status = false, message = "Ng??i d�ng ch?a x�c th?c.", data = (object?)null });

        try
        {
            var ok = await _service.DeleteAsync(userId, storyId);
            return ok
                ? Ok(new { status = true, message = "X�a bookmark th�nh c�ng.", data = new { storyId } })
                : NotFound(new { status = false, message = "Kh�ng t�m th?y bookmark ?? x�a.", data = (object?)null });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { status = false, message = ex.Message, data = (object?)null });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { status = false, message = ex.Message, data = (object?)null });
        }
    }

    private static Dictionary<string, string[]> ToErrorDict(ModelStateDictionary modelState)
        => modelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                k => k.Key,
                v => v.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "D? li?u kh�ng h?p l?." : e.ErrorMessage).ToArray()
            );
}
