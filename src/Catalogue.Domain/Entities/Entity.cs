﻿namespace Catalogue.Domain.Entities;

public abstract class Entity
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
}

