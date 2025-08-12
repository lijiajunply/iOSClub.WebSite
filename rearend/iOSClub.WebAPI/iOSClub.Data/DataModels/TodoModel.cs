﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iOSClub.Data.DataModels;

public class TodoModel : DataModel, ITodo
{
    [Column(TypeName = "varchar(20)")] public string Title { get; set; } = "";
    [Column(TypeName = "varchar(200)")] public string Description { get; set; } = "";
    [Column(TypeName = "varchar(20)")] public string StartTime { get; set; } = "";
    [Column(TypeName = "varchar(20)")] public string EndTime { get; set; } = "";

    [Column(TypeName = "boolean")] public bool Status { get; set; }

    [Key]
    [Column(TypeName = "varchar(33)")]
    public string Id { get; set; } = "";

    public StudentModel Student { get; set; } = new();

    [Column(TypeName = "varchar(10)")] public string StudentId { get; set; } = "";
    
    public void Update(ITodo model)
    {
        if (!string.IsNullOrEmpty(model.Title)) Title = model.Title;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.StartTime)) StartTime = model.StartTime;
        if (!string.IsNullOrEmpty(model.EndTime)) EndTime = model.EndTime;
        Status = model.Status;
    }
}