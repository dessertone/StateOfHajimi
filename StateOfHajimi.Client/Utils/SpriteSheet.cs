using Avalonia;
using Avalonia.Media.Imaging;

namespace StateOfHajimi.Client.Utils;

public class SpriteSheet
{
    public Bitmap Texture { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    
    private int _columns; 

    public SpriteSheet(Bitmap texture, int frameWidth, int frameHeight)
    {
        Texture = texture;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        _columns = (int)(texture.Size.Width / frameWidth);
    }

    /// <summary>
    /// 获取每一帧在图片上的裁剪区域
    /// </summary>
    public Rect GetTextureRect(int index)
    {
        int row = index / _columns;
        int col = index % _columns;

        return new Rect(
            col * FrameWidth, 
            row * FrameHeight, 
            FrameWidth, 
            FrameHeight
        );
    }
}