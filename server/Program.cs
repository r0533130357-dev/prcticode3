using Microsoft.EntityFrameworkCore;
using TodoApi; // מביא את הקלאסים שנוצרו מה-scaffold

var builder = WebApplication.CreateBuilder(args);

// ------------------- הגדרת DbContext -------------------
builder.Services.AddDbContext<ToDoDbContext>(options =>
options.UseMySql(
builder.Configuration.GetConnectionString("ToDoDB"),
ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("ToDoDB"))
)
);
// -------------------------------------------------------

// ------------------- הגדרת CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("[https://prcticode3.onrender.com](https://prcticode3.onrender.com)") // הדומיין שלך
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
// -------------------------------------------------------

// ------------------- הגדרת Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// -------------------------------------------------------

var app = builder.Build();

// הפעלת Swagger ב-Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// הפעלת מדיניות ה-CORS
app.UseCors("AllowFrontend");

// Route לדוגמא בסיסי
app.MapGet("/", () => "Hello World!");

// ------------------- ROUTES של ToDo -------------------

// 1️⃣ שליפת כל המשימות
app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    return await context.Items.ToListAsync();
});

// 2️⃣ הוספת משימה חדשה
app.MapPost("/tasks", async (Item task, ToDoDbContext context) =>
{
    context.Items.Add(task);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

// 3️⃣ עדכון משימה קיימת
app.MapPut("/tasks/{id}", async (int id, Item updatedTask, ToDoDbContext context) =>
{
    var task = await context.Items.FindAsync(id);
    if (task == null) return Results.NotFound();

task.Name = updatedTask.Name;
    task.IsComplete = updatedTask.IsComplete;

    await context.SaveChangesAsync();
    return Results.NoContent();

});

// 4️⃣ מחיקת משימה
app.MapDelete("/tasks/{id}", async (int id, ToDoDbContext context) =>
{
    var task = await context.Items.FindAsync(id);
    if (task == null) return Results.NotFound();


context.Items.Remove(task);
    await context.SaveChangesAsync();
    return Results.NoContent();

});
// -------------------------------------------------------

app.Run();
