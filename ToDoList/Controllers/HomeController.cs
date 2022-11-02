using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoList.Models;

namespace ToDoList.Controllers
{
    public class HomeController : Controller
    {
        private ToDoContext context;
        public HomeController(ToDoContext ctx) => context = ctx;

        public IActionResult Index(string id)
        {
            ToDoViewModel toDoViewModel = new ToDoViewModel();

            // load current filters and data needed for filter drop downs in ViewBag
            var filters = new Filters(id);
            toDoViewModel.Filters = filters;
            toDoViewModel.Categories = context.Categories.ToList();
            toDoViewModel.Statuses = context.Statuses.ToList();
            toDoViewModel.DueFilters = Filters.DueFilterValues;

            // get ToDo objects from database based on current filters
            IQueryable<ToDo> query = context.ToDos
                .Include(t => t.Category).Include(t => t.Status);
            if (filters.HasCategory) {
                query = query.Where(t => t.CategoryId == filters.CategoryId);
            }
            if (filters.HasStatus) {
                query = query.Where(t => t.StatusId == filters.StatusId);
            }
            if (filters.HasDue) {
                var today = DateTime.Today;
                if (filters.IsPast)
                    query = query.Where(t => t.DueDate < today);
                else if (filters.IsFuture)
                    query = query.Where(t => t.DueDate > today);
                else if (filters.IsToday)
                    query = query.Where(t => t.DueDate == today);
            }
            var tasks = query.OrderBy(t => t.DueDate).ToList();
            return View(toDoViewModel);
        }

        public IActionResult Add()
        {
            ToDoViewModel toDoViewModel = new ToDoViewModel();

            toDoViewModel.Categories = context.Categories.ToList();
            toDoViewModel.Statuses = context.Statuses.ToList();
            return View(toDoViewModel);
        }

        [HttpPost]
        public IActionResult Add(ToDoViewModel toDoViewModel)
        {
            if (ModelState.IsValid)
            {
                context.ToDos.Add(toDoViewModel.CurrentTask);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                toDoViewModel.Categories = context.Categories.ToList();
                toDoViewModel.Statuses = context.Statuses.ToList();
                return View(toDoViewModel);
            }
        }

        [HttpPost]
        public IActionResult Filter(string[] filter)
        {
            string id = string.Join('-', filter);
            return RedirectToAction("Index", new { ID = id });
        }

        [HttpPost]
        public IActionResult Edit([FromRoute]string id, ToDo selected)
        {
            if (selected.StatusId == null) {
                context.ToDos.Remove(selected);
            }
            else {
                string newStatusId = selected.StatusId;
                selected = context.ToDos.Find(selected.Id);
                selected.StatusId = newStatusId;
                context.ToDos.Update(selected);
            }
            context.SaveChanges();

            return RedirectToAction("Index", new { ID = id });
        }
    }
}