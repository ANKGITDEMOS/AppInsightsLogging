using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Net;

namespace DotNetCoreSqlDb.Controllers
{
    public class TodosController : Controller
    {
        private readonly MyDatabaseContext _context;

        private readonly ILogger<TodosController> _logger;



        public TodosController(MyDatabaseContext context, ILogger<TodosController> logger)
        {
            _logger = logger;
            _logger.LogInformation("Setting up context");
            _context = context;
        }

        // GET: Todos
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Index action called.");
            return View(await _context.Todo.ToListAsync());
        }

        // GET: Todos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("ID is not found while fetching details.");
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.ID == id);
            if (todo == null)
            {
                return NotFound();
            }
            _logger.LogInformation("ID is found ");
            return View(todo);
        }

        // GET: Todos/Create
        public IActionResult Create()
        {
            string guid = Guid.NewGuid().ToString();
            try
            {
                string s = string.Format(@"{{""name"":""Joseph {0}""}}", Guid.NewGuid().ToString());
                _context.ExecuteSP();
                using (var client = new HttpClient())
                {
                    _logger.LogInformation("Calling Logic Apps.");
                    var content = new StringContent(s, Encoding.UTF8, "application/json");

                    var response = client.PostAsync("https://XXXXXXXXXX.logic.azure.com:443/workflows/YYYYYYYYYYYYY/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=hQYQhpnMLwT6LHZSmj8HJPHR6_Xh7FOS8dBXzuTkzZw", content).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        _logger.LogInformation(string.Format("Logic Apps called successfully with runid {0} and correlationid {1}.", ((string[])response.Headers.GetValues("x-ms-workflow-run-id"))[0], ((string[])response.Headers.GetValues("x-ms-correlation-id"))[0]));
                    else
                        throw new Exception(string.Format("Logic Apps called failed with status code {0}.",response.StatusCode.ToString()));
                    
                }
                _logger.LogInformation("Create called successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in calling Create View : " + guid);
                throw new WebException("Exception occured :" + guid);
            }
            return View();
        }

        // POST: Todos/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Description,CreatedDate")] Todo todo)
        {


            _logger.LogInformation("Async create called successfully.");

            if (ModelState.IsValid)
            {
                _context.Add(todo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo.FindAsync(id);
            if (todo == null)
            {
                return NotFound();
            }
            return View(todo);
        }

        // POST: Todos/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Description,CreatedDate")] Todo todo)
        {
            if (id != todo.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(todo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TodoExists(todo.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(todo);
        }

        // GET: Todos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var todo = await _context.Todo
                .FirstOrDefaultAsync(m => m.ID == id);
            if (todo == null)
            {
                return NotFound();
            }

            return View(todo);
        }

        // POST: Todos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var todo = await _context.Todo.FindAsync(id);
            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TodoExists(int id)
        {
            return _context.Todo.Any(e => e.ID == id);
        }
    }
}
