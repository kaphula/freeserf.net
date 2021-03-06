﻿using Freeserf.Data;

namespace Freeserf.Render
{
    public interface ITextureAtlas
    {
        Texture Texture
        {
            get;
        }

        Position GetOffset(uint spriteIndex);
    }

    public interface ITextureAtlasBuilder
    {
        void AddSprite(uint spriteIndex, Sprite sprite);
        ITextureAtlas Create();
    }

    public interface ITextureAtlasBuilderFactory
    {
        ITextureAtlasBuilder Create();
    }
}
