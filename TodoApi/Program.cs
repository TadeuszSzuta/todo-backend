using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TodoApi;
using ToDoApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapHub<TodoHub>("/todoHub");

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());

app.MapPost("/todoitems", async (Todo todo, TodoDb db, IHubContext<TodoHub> hubContext) =>
{
    db.Todos.Add(todo);

    await db.SaveChangesAsync();
    await hubContext.Clients.All.SendAsync("TodosUpdated");

    return Results.Created($"/todoitems/{todo.Id}", todo);
});

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db, IHubContext < TodoHub > hubContext)=>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();
    await hubContext.Clients.All.SendAsync("TodosUpdated");

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db, IHubContext<TodoHub> hubContext) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        await hubContext.Clients.All.SendAsync("TodosUpdated");
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();
