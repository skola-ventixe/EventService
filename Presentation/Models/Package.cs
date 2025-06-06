﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Presentation.Models;

public class Package
{
    public string? Id { get; set; }
    public string? EventId { get; set; } = null!;
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; } = null!;

    public bool Seated { get; set; }
    public string Placement { get; set; } = null!;

    public List<Benefit>? Benefits { get; set; } = [];

}
