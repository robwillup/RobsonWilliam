using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Rob.Api.Mongo;
using System;
using System.Net;
using Rob.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors();
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseCors();

app.MapGet("/articles", (async contex =>
{
    var dbConn = new DbConnector(app.Configuration["ConnStr"]);
    List<Article> articles = await dbConn.GetAllDocs();
    await contex.Response.WriteAsJsonAsync(articles);
}));

app.MapGet("/articles/{id}", (async context =>
{
    var dbConn = new DbConnector(app.Configuration["ConnStr"]);
    string id = context.Request.RouteValues["id"].ToString();
    Article result = await dbConn.GetOneDocByTitleAsync(id);
    await context.Response.WriteAsJsonAsync(result);
})).RequireCors(op => 
{
    op.AllowAnyOrigin();
});

app.MapPost("/articles", async context =>
{    
    if(!context.Request.HasJsonContentType())
    {
        context.Response.StatusCode = (int) HttpStatusCode.UnsupportedMediaType;
        return;
    }
    var post = await context.Request.ReadFromJsonAsync<Article>();
    var dbConn = new DbConnector(app.Configuration["ConnStr"]);
    await dbConn.InsertOneDocAsync(post);
    context.Response.StatusCode = (int) HttpStatusCode.Accepted;
});

app.MapGet("/", (Func<string>)( () =>
{
    return "\"For even the very wise cannot see all ends.\" -- Gandalf";
}));

await app.RunAsync();
