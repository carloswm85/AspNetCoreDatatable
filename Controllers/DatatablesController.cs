using AspNetCoreDatatable.Data;
using AspNetCoreDatatable.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreDatatable.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DatatablesController : ControllerBase
	{
		private readonly AspNetCoreDatatableContext _context;
		private readonly ILogger<DatatablesController> _logger;

		public DatatablesController(AspNetCoreDatatableContext context, ILogger<DatatablesController> logger)
		{
			_context = context;
			_logger = logger;
		}

		// Basic Datatable
		[HttpGet]
		public async Task<IActionResult> GetAsync()
		{
			Debug.WriteLine("> BASIC");
			var watch = Stopwatch.StartNew();
			watch.Start();
			var data = await _context.UserInfos.ToListAsync();
			Debug.WriteLine($"Elapsed time: {watch.Elapsed}");
			Debug.WriteLine($"Items count: {data.Count}");
			return Ok(new { data });
		}

		// Pagination
		[HttpPost("pagination")]
		public async Task<IActionResult> PostPaginationAsync()
		{
			Debug.WriteLine("> PAGINATION");
			try
			{
				var watch = Stopwatch.StartNew();
				watch.Start();

				string draw = Request.Form["draw"].FirstOrDefault();
				string start = Request.Form["start"].FirstOrDefault();
				string length = Request.Form["length"].FirstOrDefault();
				string searchValue = Request.Form["search[value]"].FirstOrDefault();
				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;

				IQueryable<UserInfo> userInfo = (from dbuserinfo in _context.UserInfos select dbuserinfo);
				if (!string.IsNullOrEmpty(searchValue))
				{
					userInfo = userInfo.Where(m => m.Name.Contains(searchValue)
												|| m.Gender.Contains(searchValue)
												|| m.EyeColor.Contains(searchValue)
												|| m.Email.Contains(searchValue)
												|| m.Phone.Contains(searchValue)
												|| m.Company.Contains(searchValue));
				}

				recordsTotal = userInfo.Count();
				Debug.WriteLine($"Items count: {recordsTotal}");

				List<UserInfo> data = new List<UserInfo>();

				if (pageSize < 0)
				{
					data = await userInfo.ToListAsync();
				}
				else
				{
					data = await userInfo.Skip(skip).Take(pageSize).ToListAsync();
				}

				Debug.WriteLine($"Elapsed time: {watch.Elapsed}");
				return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				_logger.LogError(ex, "An error occurred during processing.");
				return BadRequest();
				throw;
			}
		}

		// Searching
		[HttpPost("searching")]
		public async Task<IActionResult> PostSearchingAsync()
		{
			Debug.WriteLine("> SEARCHING");
			try
			{
				var watch = Stopwatch.StartNew();
				watch.Start();

				string draw = Request.Form["draw"].FirstOrDefault();
				string start = Request.Form["start"].FirstOrDefault();
				string length = Request.Form["length"].FirstOrDefault();
				string searchValue = Request.Form["search[value]"].FirstOrDefault();
				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;

				IQueryable<UserInfo> userInfo = (from dbuserinfo in _context.UserInfos select dbuserinfo);
				if (!string.IsNullOrEmpty(searchValue))
				{
					userInfo = userInfo.Where(m => m.Name.Contains(searchValue)
												|| m.Gender.Contains(searchValue)
												|| m.EyeColor.Contains(searchValue)
												|| m.Email.Contains(searchValue)
												|| m.Phone.Contains(searchValue)
												|| m.Company.Contains(searchValue));
				}

				recordsTotal = userInfo.Count();
				Debug.WriteLine(recordsTotal);

				List<UserInfo> data = new List<UserInfo>();

				if (pageSize < 0)
				{
					data = await userInfo.ToListAsync();
				}
				else
				{
					data = await userInfo.Skip(skip).Take(pageSize).ToListAsync();
				}

				Debug.WriteLine($"Elapsed time: {watch.Elapsed}");
				return Ok(new { draw, recordsFiltered = recordsTotal, recordsTotal, data });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during processing.");
				return BadRequest();
				throw;
			}
		}

		// Ordering
		[HttpPost("ordering")]
		public async Task<IActionResult> PostOrderingAsync()
		{
			Debug.WriteLine("> ORDERING");
			try
			{
				var watch = Stopwatch.StartNew();
				watch.Start();

				// Create a config object
				ParsingConfig config = new ParsingConfig
				{
					UseParameterizedNamesInDynamicQuery = true
				};

				string draw = Request.Form["draw"].FirstOrDefault();
				string start = Request.Form["start"].FirstOrDefault();
				string length = Request.Form["length"].FirstOrDefault();
				string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
				string sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
				string searchValue = Request.Form["search[value]"].FirstOrDefault();
				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;

				IQueryable<UserInfo> userInfo = (from dbUserInfo in _context.UserInfos select dbUserInfo);
				recordsTotal = userInfo.Count();
				Debug.WriteLine(recordsTotal);

				if (!string.IsNullOrEmpty(searchValue))
				{
					userInfo = userInfo.Where(m => m.Name.Contains(searchValue)
												|| m.Gender.Contains(searchValue)
												|| m.EyeColor.Contains(searchValue)
												|| m.Email.Contains(searchValue)
												|| m.Phone.Contains(searchValue)
												|| m.Company.Contains(searchValue));
				}

				if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
				{
					userInfo = userInfo.OrderBy(sortColumn + " " + sortColumnDirection);
				}

				List<UserInfo> data = new List<UserInfo>();

				if (pageSize < 0)
				{
					data = await userInfo.ToListAsync();
				}
				else
				{
					data = await userInfo.Skip(skip).Take(pageSize).ToListAsync();
				}

				Debug.WriteLine($"Elapsed time: {watch.Elapsed}");
				var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
				return Ok(jsonData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during processing.");
				return BadRequest();
				throw;
			}
		}

		// All IN One
		[HttpPost("AIO")]
		public async Task<IActionResult> PostAIOAsync()
		{
			Debug.WriteLine("> AIO");
			try
			{
				var watch = Stopwatch.StartNew();
				watch.Start();

				// Create a config object
				ParsingConfig config = new ParsingConfig
				{
					UseParameterizedNamesInDynamicQuery = true
				};

				string draw = Request.Form["draw"].FirstOrDefault();
				string start = Request.Form["start"].FirstOrDefault();
				string length = Request.Form["length"].FirstOrDefault();
				string sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
				string sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
				string searchValue = Request.Form["search[value]"].FirstOrDefault();
				int pageSize = length != null ? Convert.ToInt32(length) : 0;
				int skip = start != null ? Convert.ToInt32(start) : 0;
				int recordsTotal = 0;

				IQueryable<UserInfo> userInfo = (from dbUserInfo in _context.UserInfos select dbUserInfo);
				recordsTotal = userInfo.Count();

				StringBuilder @condition = new StringBuilder();
				@condition.Append("u => ");
				PropertyInfo[] props = typeof(UserInfo).GetProperties();
				for (int i = 0; i < props.Length; i++)
				{
					if (!string.IsNullOrEmpty(searchValue) && (props[i].Name == "Name" || props[i].Name == "Age" || props[i].Name == "IsActive"))
					{
						if (props[i].PropertyType != typeof(string))
						{
							@condition.Append("u.");
							@condition.Append(props[i].Name);
							@condition.Append(".Value.ToString().Contains(\"" + searchValue + "\") || ");
						}
						else
						{
							@condition.Append("u.");
							@condition.Append(props[i].Name);
							@condition.Append(".Contains(\"" + searchValue + "\") || ");
						}
					}
				}
				if (!string.IsNullOrEmpty(searchValue))
				{
					var whereStr = @condition.ToString().Substring(0, @condition.Length - 4);
					userInfo = userInfo.Where(whereStr);
				}

				if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
				{
					userInfo = userInfo.OrderBy(sortColumn + " " + sortColumnDirection);
				}

				List<UserInfo> data = new List<UserInfo>();

				if (pageSize < 0)
				{
					data = await userInfo.ToListAsync();
				}
				else
				{
					data = await userInfo.Skip(skip).Take(pageSize).ToListAsync();
				}

				Debug.WriteLine($"Elapsed time: {watch.Elapsed}");
				var jsonData = new { draw, recordsFiltered = recordsTotal, recordsTotal, data };
				return Ok(jsonData);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occurred during processing.");
				return BadRequest();
				throw;
			}
		}
	}
}
