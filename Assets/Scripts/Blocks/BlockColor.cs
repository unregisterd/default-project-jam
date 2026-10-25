public enum BlockColor
{
    None = 0,
    Red = 1,
    Blue = 2,
}

public static class BlockColorExtensions
{
    public static BlockColor Opposite(this BlockColor color)
    {
        switch (color)
        {
            case BlockColor.Red:
                return BlockColor.Blue;
            case BlockColor.Blue:
                return BlockColor.Red;
            default:
                return BlockColor.None;
        }
    }
}
