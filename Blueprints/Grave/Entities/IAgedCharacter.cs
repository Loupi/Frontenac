﻿namespace Grave.Entities
{
    public interface IAgedCharacter : ICharacter
    {
        int Age { get; set; }
    }
}