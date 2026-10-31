using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

public static class PanelSkin
{
    // ============================================================ 主题
    public static string ThemeName = "Panel Rounded (Dark)";

    // 中性灰 -> 输出 RGB 锚点 (input, r, g, b)
    public static int[][] A = new int[][] {
        new int[]{  0,  14,  16,  20},   // 窗口描边（原纯黑，改为柔和深灰）
        new int[]{ 16,  18,  21,  26},   // #101010 视频底
        new int[]{ 21,  22,  25,  30},   // #151515 未激活标题栏
        new int[]{ 24,  25,  28,  34},   // #181818 按下态
        new int[]{ 30,  33,  37,  45},   // #1E1E1E 面板底
        new int[]{ 39,  42,  47,  56},   // #272727 悬停态
        new int[]{ 51,  53,  59,  70},
        new int[]{ 68,  66,  73,  86},
        new int[]{ 90,  86,  94, 108},
        new int[]{119, 120, 129, 145},
        new int[]{139, 140, 149, 165},
        new int[]{160, 162, 170, 185},
        new int[]{200, 206, 212, 224},
        new int[]{234, 236, 240, 247},
        new int[]{240, 238, 242, 249},   // #F0F0F0 图标/文字
        new int[]{255, 255, 255, 255}
    };

    public static double AccentHue    = 188.0;
    public static double AccentHueSrc = 54.0;
    public static double AccentSatK   = 0.84;
    public static double AccentLumK   = 1.09;
    public static double ChromaMin    = 0.080;
    public static double YellowLo     = 40.0;
    public static double YellowHi     = 85.0;
    public static int TintR = 0, TintG = 0, TintB = 0;

    // 面板显式色（横向一致，安全）
    public static Color CBorder     = Color.FromArgb(255, 0x22, 0x25, 0x2B);
    public static Color CVideo      = Color.FromArgb(255, 0x0C, 0x0E, 0x11);
    public static Color CTitle      = Color.FromArgb(255, 0x18, 0x1B, 0x20);
    public static Color CSeek       = Color.FromArgb(255, 0x13, 0x15, 0x19);
    public static Color CBar        = Color.FromArgb(255, 0x1F, 0x22, 0x28);
    public static Color CFiller     = Color.FromArgb(255, 0x1B, 0x1E, 0x23);
    public static Color CTitleTop1  = Color.FromArgb(255, 0x2C, 0x32, 0x3B);
    public static Color CTitleTop2  = Color.FromArgb(255, 0x22, 0x27, 0x2E);
    public static Color CAccentLine = Color.FromArgb(255, 0x17, 0x6E, 0x80);

    public static int ChipFill = 10;      // 卡片相对底色的填充增量
    public static int ChipBorder = 26;    // 卡片描边增量
    public static int CardRadius = 9;
    public static int CardInset = 3;

    // macOS 风格：柔和投影 + 大卡片
    public static int ShadowAlpha = 0, ShadowBlur = 0, ShadowDY = 0;
    public static Color CShadow   = Color.Black;
    public static Color CWindowBg = Color.FromArgb(255, 0x1B, 0x1E, 0x23);
    public static Color CDivider  = Color.FromArgb(255, 0x2B, 0x2F, 0x36);
    public static bool  RoundBars = false;
    public static int   BarRadius = 10;

    // 滚动条专属配色（源图色域很窄，套通用色阶会全部挤成浅灰）
    public static Color CScrollTrack = Color.FromArgb(255, 0x2C, 0x2C, 0x30);
    public static Color CScrollThumb = Color.FromArgb(255, 0x6E, 0x6E, 0x76);
    public static bool  ScrollBarMode = false;
    static int ScrollThumbK = -1;   // 滑块源色（从滑块单元格里取）
    static int ScrollTrackK = -1;   // 轨道源色（全图众数）
    // 卡片外圈填充色（默认透明）。若元素背后没有底色，必须给一个不透明色，否则透明处会显示为黑。
    public static Color CardBg   = Color.Transparent;
    // 卡片填充覆盖色（A>0 时替代由底色推导的填充）
    public static Color CardFill = Color.Transparent;
    // “白底页面”型图集：只换强调色，不反转明暗（否则浅色主题会把整页压成黑的）
    public static bool  IdentityNeutral = false;

    public static void UseDark()
    {
        ThemeName = "Panel Rounded Dark";
        A = new int[][] {
            new int[]{  0,  14,  16,  20}, new int[]{ 16,  18,  21,  26},
            new int[]{ 21,  22,  25,  30}, new int[]{ 24,  25,  28,  34},
            new int[]{ 30,  33,  37,  45}, new int[]{ 39,  42,  47,  56},
            new int[]{ 51,  53,  59,  70}, new int[]{ 68,  66,  73,  86},
            new int[]{ 90,  86,  94, 108}, new int[]{119, 120, 129, 145},
            new int[]{139, 140, 149, 165}, new int[]{160, 162, 170, 185},
            new int[]{200, 206, 212, 224}, new int[]{234, 236, 240, 247},
            new int[]{240, 238, 242, 249}, new int[]{255, 255, 255, 255}
        };
        CBorder = Color.FromArgb(255, 0x22, 0x25, 0x2B);
        CVideo  = Color.FromArgb(255, 0x0C, 0x0E, 0x11);
        CTitle  = Color.FromArgb(255, 0x18, 0x1B, 0x20);
        CSeek   = Color.FromArgb(255, 0x13, 0x15, 0x19);
        CBar    = Color.FromArgb(255, 0x1F, 0x22, 0x28);
        CFiller = Color.FromArgb(255, 0x1B, 0x1E, 0x23);
        CTitleTop1 = Color.FromArgb(255, 0x2C, 0x32, 0x3B);
        CTitleTop2 = Color.FromArgb(255, 0x22, 0x27, 0x2E);
        CAccentLine = Color.FromArgb(255, 0x17, 0x6E, 0x80);
        CScrollTrack = Color.FromArgb(255, 0x3B, 0x42, 0x4E);   // = 列表底色
        CScrollThumb = Color.FromArgb(255, 0x6A, 0x74, 0x84);
        ChipFill = 12; ChipBorder = 30; CardRadius = 9; CardInset = 3;
    }

    public static void UseLight()
    {
        ThemeName = "Panel Rounded Light";
        A = new int[][] {
            new int[]{  0, 0xE2, 0xE6, 0xEC},   // 窗口描边 -> 浅灰
            new int[]{ 16, 0xF4, 0xF6, 0xF9},   // 视频底
            new int[]{ 21, 0xF7, 0xF9, 0xFB},
            new int[]{ 24, 0xEC, 0xEF, 0xF3},
            new int[]{ 30, 0xFF, 0xFF, 0xFF},   // 面板 -> 白卡
            new int[]{ 39, 0xF3, 0xF5, 0xF8},   // 悬停
            new int[]{ 51, 0xEB, 0xEE, 0xF2},
            new int[]{ 68, 0xE4, 0xE8, 0xEE},
            new int[]{ 90, 0xD2, 0xD8, 0xE0},
            new int[]{119, 0x9A, 0xA3, 0xAF},   // 小图标
            new int[]{139, 0x86, 0x90, 0x9D},
            new int[]{160, 0x70, 0x7A, 0x88},
            new int[]{200, 0x4E, 0x57, 0x63},
            new int[]{234, 0x38, 0x3F, 0x49},
            new int[]{240, 0x2C, 0x33, 0x3C},   // 图标/文字 -> 深
            new int[]{255, 0x1F, 0x25, 0x2D}
        };
        AccentHue = 215.0; AccentSatK = 0.92; AccentLumK = 1.22;
        CBorder = Color.FromArgb(255, 0xE3, 0xE7, 0xED);
        CVideo  = Color.FromArgb(255, 0xF5, 0xF7, 0xFA);
        CTitle  = Color.FromArgb(255, 0xFF, 0xFF, 0xFF);
        CSeek   = Color.FromArgb(255, 0xF7, 0xF9, 0xFB);
        CBar    = Color.FromArgb(255, 0xFF, 0xFF, 0xFF);
        CFiller = Color.FromArgb(255, 0xFC, 0xFD, 0xFE);
        CTitleTop1 = Color.FromArgb(255, 0xFF, 0xFF, 0xFF);
        CTitleTop2 = Color.FromArgb(255, 0xFA, 0xFB, 0xFD);
        CAccentLine = Color.FromArgb(255, 0xBF, 0xD6, 0xF5);
        CScrollTrack = Color.FromArgb(255, 0xE8, 0xEB, 0xF0);   // = 列表底色
        CScrollThumb = Color.FromArgb(255, 0xA6, 0xAC, 0xB6);
        ChipFill = 0; ChipBorder = -18; CardRadius = 10; CardInset = 3;
    }

    public static void UseMacLight()
    {
        ThemeName = "macOS Light";
        A = new int[][] {
            new int[]{  0, 0xD8, 0xD8, 0xDD},   // 外描边 -> 发丝灰
            new int[]{ 16, 0xEC, 0xEC, 0xEF},   // 画面底 / 窗口底
            new int[]{ 21, 0xF2, 0xF2, 0xF5},
            new int[]{ 24, 0xE4, 0xE4, 0xE9},   // 按下
            new int[]{ 30, 0xFF, 0xFF, 0xFF},   // 面板 -> 白卡
            new int[]{ 39, 0xF2, 0xF2, 0xF5},   // 悬停
            new int[]{ 51, 0xE9, 0xE9, 0xEE},
            new int[]{ 68, 0xE1, 0xE1, 0xE6},
            new int[]{ 90, 0xD5, 0xD5, 0xDA},
            new int[]{119, 0x8E, 0x8E, 0x93},   // macOS systemGray
            new int[]{139, 0x7C, 0x7C, 0x81},
            new int[]{160, 0x6A, 0x6A, 0x6F},
            new int[]{200, 0x48, 0x48, 0x4D},
            new int[]{234, 0x2E, 0x2E, 0x32},
            new int[]{240, 0x1D, 0x1D, 0x1F},   // macOS label
            new int[]{255, 0x17, 0x17, 0x19}
        };
        AccentHue = 211.0; AccentSatK = 1.00; AccentLumK = 1.02;
        CBorder    = Color.FromArgb(255, 0xD8, 0xD8, 0xDD);
        CVideo     = Color.FromArgb(255, 0xEC, 0xEC, 0xEF);
        CTitle     = Color.FromArgb(255, 0xF7, 0xF7, 0xF9);
        CSeek      = Color.FromArgb(255, 0xF2, 0xF2, 0xF5);
        CBar       = Color.FromArgb(255, 0xF2, 0xF2, 0xF5);
        CFiller    = Color.FromArgb(255, 0xEC, 0xEC, 0xEF);
        CTitleTop1 = CTitle; CTitleTop2 = CTitle;
        CAccentLine= Color.FromArgb(255, 0xE2, 0xE2, 0xE7);
        CWindowBg  = Color.FromArgb(255, 0xEC, 0xEC, 0xEF);
        CDivider   = Color.FromArgb(255, 0xE6, 0xE6, 0xEB);
        CScrollTrack = Color.FromArgb(255, 0xE5, 0xE5, 0xEA);   // = 列表底色
        CScrollThumb = Color.FromArgb(255, 0xA8, 0xA8, 0xB0);
        ChipFill = 0; ChipBorder = -20; CardRadius = 12; CardInset = 4;
        ShadowAlpha = 34; ShadowBlur = 4; ShadowDY = 1;
        RoundBars = true; BarRadius = 10;
    }

    public static void UseMacDark()
    {
        ThemeName = "macOS Dark";
        A = new int[][] {
            new int[]{  0, 0x3A, 0x3A, 0x3C},
            new int[]{ 16, 0x1C, 0x1C, 0x1E},
            new int[]{ 21, 0x23, 0x23, 0x26},
            new int[]{ 24, 0x26, 0x26, 0x2A},
            new int[]{ 30, 0x2C, 0x2C, 0x2E},
            new int[]{ 39, 0x3A, 0x3A, 0x3C},
            new int[]{ 51, 0x3F, 0x3F, 0x42},
            new int[]{ 68, 0x48, 0x48, 0x4A},
            new int[]{ 90, 0x5C, 0x5C, 0x60},
            new int[]{119, 0x8E, 0x8E, 0x93},
            new int[]{139, 0xA2, 0xA2, 0xA7},
            new int[]{160, 0xB6, 0xB6, 0xBB},
            new int[]{200, 0xD6, 0xD6, 0xDB},
            new int[]{234, 0xEC, 0xEC, 0xF0},
            new int[]{240, 0xF5, 0xF5, 0xF7},
            new int[]{255, 0xFF, 0xFF, 0xFF}
        };
        AccentHue = 210.0; AccentSatK = 1.00; AccentLumK = 1.06;
        CBorder    = Color.FromArgb(255, 0x3A, 0x3A, 0x3C);
        CVideo     = Color.FromArgb(255, 0x1C, 0x1C, 0x1E);
        CTitle     = Color.FromArgb(255, 0x26, 0x26, 0x2A);
        CSeek      = Color.FromArgb(255, 0x23, 0x23, 0x27);
        CBar       = Color.FromArgb(255, 0x23, 0x23, 0x27);
        CFiller    = Color.FromArgb(255, 0x1C, 0x1C, 0x1E);
        CTitleTop1 = CTitle; CTitleTop2 = CTitle;
        CAccentLine= Color.FromArgb(255, 0x3A, 0x3A, 0x3C);
        CWindowBg  = Color.FromArgb(255, 0x1C, 0x1C, 0x1E);
        CDivider   = Color.FromArgb(255, 0x3A, 0x3A, 0x3C);
        CScrollTrack = Color.FromArgb(255, 0x43, 0x43, 0x46);   // = 列表底色
        CScrollThumb = Color.FromArgb(255, 0x6E, 0x6E, 0x74);
        ChipFill = 0; ChipBorder = 22; CardRadius = 12; CardInset = 4;
        ShadowAlpha = 70; ShadowBlur = 5; ShadowDY = 2;
        RoundBars = true; BarRadius = 10;
    }

    static int Clamp(double v) { int i = (int)Math.Round(v); return i < 0 ? 0 : (i > 255 ? 255 : i); }

    // ============================================================ 颜色映射
    static double[] MapNeutral(double v)
    {
        int n = A.Length;
        if (v <= A[0][0])   return new double[]{ A[0][1], A[0][2], A[0][3] };
        if (v >= A[n-1][0]) return new double[]{ A[n-1][1], A[n-1][2], A[n-1][3] };
        for (int i = 0; i < n - 1; i++) {
            if (v >= A[i][0] && v <= A[i+1][0]) {
                double t = (v - A[i][0]) / (double)(A[i+1][0] - A[i][0]);
                return new double[]{
                    A[i][1] + t*(A[i+1][1]-A[i][1]),
                    A[i][2] + t*(A[i+1][2]-A[i][2]),
                    A[i][3] + t*(A[i+1][3]-A[i][3]) };
            }
        }
        return new double[]{ v, v, v };
    }

    static void RgbToHsl(int r, int g, int b, out double h, out double s, out double l)
    {
        double R = r/255.0, G = g/255.0, B = b/255.0;
        double mx = Math.Max(R, Math.Max(G, B)), mn = Math.Min(R, Math.Min(G, B));
        l = (mx + mn) / 2.0; double d = mx - mn;
        if (d < 1e-9) { h = 0; s = 0; return; }
        s = l > 0.5 ? d/(2.0-mx-mn) : d/(mx+mn);
        if (mx == R)      h = 60.0 * (((G-B)/d) % 6.0);
        else if (mx == G) h = 60.0 * (((B-R)/d) + 2.0);
        else              h = 60.0 * (((R-G)/d) + 4.0);
        if (h < 0) h += 360.0;
    }

    static double Hue2Rgb(double p, double q, double t)
    {
        if (t < 0) t += 1; if (t > 1) t -= 1;
        if (t < 1.0/6) return p + (q-p)*6*t;
        if (t < 1.0/2) return q;
        if (t < 2.0/3) return p + (q-p)*(2.0/3-t)*6;
        return p;
    }

    static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
    {
        if (s <= 1e-9) { int v = Clamp(l*255); r = v; g = v; b = v; return; }
        double q = l < 0.5 ? l*(1+s) : l+s-l*s, p = 2*l-q;
        double hk = (((h % 360) + 360) % 360) / 360.0;
        r = Clamp(Hue2Rgb(p,q,hk+1.0/3)*255);
        g = Clamp(Hue2Rgb(p,q,hk)*255);
        b = Clamp(Hue2Rgb(p,q,hk-1.0/3)*255);
    }

    public static void MapPixel(int r, int g, int b, out int nr, out int ng, out int nb)
    {
        int mx = Math.Max(r, Math.Max(g, b)), mn = Math.Min(r, Math.Min(g, b));
        if ((mx-mn)/255.0 < ChromaMin) {
            if (IdentityNeutral) { nr = r; ng = g; nb = b; return; }
            double lum = 0.299*r + 0.587*g + 0.114*b;
            if (ScrollBarMode) {
                // 滚动条专属：按源色的“角色”分派，而不是按亮度（源图里滑块和轨道往往只差几级灰）
                int key = (r<<16)|(g<<8)|b;
                if (key == ScrollThumbK) { nr = CScrollThumb.R; ng = CScrollThumb.G; nb = CScrollThumb.B; return; }
                if (key == ScrollTrackK) { nr = CScrollTrack.R; ng = CScrollTrack.G; nb = CScrollTrack.B; return; }
                // 其它（箭头 / 边框 / 高光）：取两者之间，保证可见
                nr = Clamp(CScrollTrack.R + (CScrollThumb.R - CScrollTrack.R) * 0.62);
                ng = Clamp(CScrollTrack.G + (CScrollThumb.G - CScrollTrack.G) * 0.62);
                nb = Clamp(CScrollTrack.B + (CScrollThumb.B - CScrollTrack.B) * 0.62);
                return;
            }
            double[] o = MapNeutral(lum);
            nr = Clamp(o[0]+TintR); ng = Clamp(o[1]+TintG); nb = Clamp(o[2]+TintB);
            return;
        }
        double h, s, l;
        RgbToHsl(r, g, b, out h, out s, out l);
        if (h >= YellowLo && h <= YellowHi) {
            HslToRgb(AccentHue + (h-AccentHueSrc)*0.5,
                     Math.Min(1.0, s*AccentSatK), Math.Min(1.0, l*AccentLumK),
                     out nr, out ng, out nb);
            return;
        }
        nr = r; ng = g; nb = b;
    }

    // ============================================================ 缓冲工具
    static int Px(byte[] b, int st, int x, int y) { int i = y*st + x*4; return (b[i+3]<<24)|(b[i+2]<<16)|(b[i+1]<<8)|b[i]; }
    static void SetPx(byte[] b, int st, int x, int y, int a, int r, int g, int bl)
    { int i = y*st + x*4; b[i]=(byte)bl; b[i+1]=(byte)g; b[i+2]=(byte)r; b[i+3]=(byte)a; }

    static void FillRect(byte[] buf, int st, int W, int H, int x0, int y0, int x1, int y1, Color c)
    {
        if (x0 < 0) x0 = 0; if (y0 < 0) y0 = 0;
        if (x1 > W-1) x1 = W-1; if (y1 > H-1) y1 = H-1;
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++) {
                int i = y*st + x*4;
                if (buf[i+3] == 0) continue;
                buf[i]=c.B; buf[i+1]=c.G; buf[i+2]=c.R;
            }
    }
    // 无条件覆盖（含 alpha），用于面板底
    static void PaintRect(byte[] buf, int st, int W, int H, int x0, int y0, int x1, int y1, Color c, bool keepTransparent)
    {
        if (x0 < 0) x0 = 0; if (y0 < 0) y0 = 0;
        if (x1 > W-1) x1 = W-1; if (y1 > H-1) y1 = H-1;
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++) {
                int i = y*st + x*4;
                if (keepTransparent && buf[i+3] == 0) continue;
                buf[i]=c.B; buf[i+1]=c.G; buf[i+2]=c.R; buf[i+3]=255;
            }
    }

    static Color PxColor(byte[] b, int st, int x, int y)
    { int k = Px(b, st, x, y); return Color.FromArgb((k>>24)&0xFF, (k>>16)&0xFF, (k>>8)&0xFF, k&0xFF); }

    static Color Adjust(Color c, int d)
    { return Color.FromArgb(c.A, Clamp(c.R+d), Clamp(c.G+d), Clamp(c.B+d)); }

    static int MaxDiff(int r1,int g1,int b1,int r2,int g2,int b2)
    { return Math.Max(Math.Abs(r1-r2), Math.Max(Math.Abs(g1-g2), Math.Abs(b1-b2))); }

    static Color Dominant(byte[] buf, int st, int x, int y, int w, int h, out double opaqueFrac)
    {
        var hist = new Dictionary<int,int>();
        int opaque = 0;
        for (int j = y; j < y+h; j++)
            for (int i = x; i < x+w; i++) {
                int k = Px(buf, st, i, j); int a = (k>>24)&0xFF;
                if (a < 8) continue;
                opaque++;
                int rgb = k & 0xFFFFFF;
                int c; hist.TryGetValue(rgb, out c); hist[rgb] = c+1;
            }
        opaqueFrac = opaque / (double)(w*h);
        int best = 0, bn = -1;
        foreach (var kv in hist) if (kv.Value > bn) { bn = kv.Value; best = kv.Key; }
        return Color.FromArgb(255, (best>>16)&0xFF, (best>>8)&0xFF, best&0xFF);
    }

    static GraphicsPath RoundPath(Rectangle r, int radius)
    {
        var p = new GraphicsPath();
        int d = Math.Max(1, radius * 2);
        if (d > r.Width) d = r.Width;
        if (d > r.Height) d = r.Height;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right-d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom-d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    // 把一张小位图以 over 方式叠加到主缓冲
    static void Blit(byte[] buf, int st, int W, int H, Bitmap bm, int dx, int dy, bool replace)
    {
        var d = bm.LockBits(new Rectangle(0,0,bm.Width,bm.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        int bs = d.Stride; byte[] sb = new byte[Math.Abs(bs)*bm.Height];
        Marshal.Copy(d.Scan0, sb, 0, sb.Length); bm.UnlockBits(d);
        for (int y = 0; y < bm.Height; y++) {
            int ty = dy+y; if (ty < 0 || ty >= H) continue;
            for (int x = 0; x < bm.Width; x++) {
                int tx = dx+x; if (tx < 0 || tx >= W) continue;
                int i = y*bs + x*4;
                int a = sb[i+3], r = sb[i+2], g = sb[i+1], b = sb[i];
                if (!replace && a == 0) continue;
                if (replace) { SetPx(buf, st, tx, ty, a, r, g, b); continue; }
                int ti = ty*st + tx*4;
                int da = buf[ti+3];
                if (a == 255 || da == 0) { buf[ti]=(byte)b; buf[ti+1]=(byte)g; buf[ti+2]=(byte)r; buf[ti+3]=(byte)a; continue; }
                double sa = a/255.0, oa = 1-sa;
                buf[ti]   = (byte)(b*sa + buf[ti]*oa);
                buf[ti+1] = (byte)(g*sa + buf[ti+1]*oa);
                buf[ti+2] = (byte)(r*sa + buf[ti+2]*oa);
                buf[ti+3] = (byte)((a + da*(255-a)/255));
            }
        }
    }

    // ============================================================ 圆角卡片化
    const int TOL = 22;

    static byte[] BoxH(byte[] src, int st, int W, int H, int r)
    {
        var dst = new byte[src.Length];
        for (int y = 0; y < H; y++) { int row = y*st;
            for (int x = 0; x < W; x++) {
                int sum = 0, cnt = 0;
                for (int k = -r; k <= r; k++) { int xx = x+k; if (xx < 0 || xx >= W) continue; sum += src[row+xx*4+3]; cnt++; }
                int i = row + x*4;
                dst[i]=src[i]; dst[i+1]=src[i+1]; dst[i+2]=src[i+2];
                dst[i+3] = (byte)(sum / Math.Max(1,cnt));
            } }
        return dst;
    }

    static byte[] BoxV(byte[] src, int st, int W, int H, int r)
    {
        var dst = new byte[src.Length];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++) {
                int sum = 0, cnt = 0;
                for (int k = -r; k <= r; k++) { int yy = y+k; if (yy < 0 || yy >= H) continue; sum += src[yy*st+x*4+3]; cnt++; }
                int i = y*st + x*4;
                dst[i]=src[i]; dst[i+1]=src[i+1]; dst[i+2]=src[i+2];
                dst[i+3] = (byte)(sum / Math.Max(1,cnt));
            }
        return dst;
    }

    // 生成柔和投影（对 alpha 做三次盒式模糊）
    static Bitmap MakeShadow(int w, int h, Rectangle card, int radius)
    {
        var sh = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(sh)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            var sr = Rectangle.Inflate(card, Math.Max(1, ShadowBlur/2), Math.Max(1, ShadowBlur/2));
            sr.Offset(0, ShadowDY);
            using (var path = RoundPath(sr, radius + ShadowBlur/2))
            using (var fb = new SolidBrush(Color.Black)) g.FillPath(fb, path);
        }
        var d = sh.LockBits(new Rectangle(0,0,w,h), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
        int st = d.Stride; byte[] buf = new byte[Math.Abs(st)*h];
        Marshal.Copy(d.Scan0, buf, 0, buf.Length);
        for (int pass = 0; pass < 3; pass++) { buf = BoxH(buf, st, w, h, ShadowBlur); buf = BoxV(buf, st, w, h, ShadowBlur); }
        for (int y = 0; y < h; y++) { int row = y*st;
            for (int x = 0; x < w; x++) { int i = row + x*4;
                buf[i] = CShadow.B; buf[i+1] = CShadow.G; buf[i+2] = CShadow.R;
                buf[i+3] = (byte)(buf[i+3] * ShadowAlpha / 255); } }
        Marshal.Copy(buf, 0, d.Scan0, buf.Length);
        sh.UnlockBits(d);
        return sh;
    }

    // 渲染一张“卡片”：可选背景 + 柔和投影 + 圆角填充 + 发丝描边
    static Bitmap RenderCard(int w, int h, Rectangle card, int radius,
                             Color fill, Color border, bool useBorder, Color bg)
    {
        var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (bg.A > 0) g.Clear(bg); else g.Clear(Color.Transparent);
            if (ShadowAlpha > 0 && ShadowBlur > 0) {
                using (var sh = MakeShadow(w, h, card, radius)) g.DrawImage(sh, 0, 0);
            }
            using (var path = RoundPath(card, radius)) {
                using (var fb = new SolidBrush(fill)) g.FillPath(fb, path);
                if (useBorder) {
                    using (var pen = new Pen(border, 1f)) { pen.Alignment = PenAlignment.Inset; g.DrawPath(pen, path); }
                }
            }
        }
        return bmp;
    }

    public static void MakeCard(byte[] buf, int st, int W, int H,
                                int x, int y, int w, int h, int inset, int radius, int fillDelta, int borderDelta)
    {
        double of;
        Color baseC = Dominant(buf, st, x, y, w, h, out of);
        if (of < 0.35) return;                       // 以图标为主，不动

        // 1. 抽出“内容”层（图标）
        using (var content = new Bitmap(w, h, PixelFormat.Format32bppArgb)) {
            var cd = content.LockBits(new Rectangle(0,0,w,h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int cs = cd.Stride; byte[] cb = new byte[Math.Abs(cs)*h];
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++) {
                    int k = Px(buf, st, x+i, y+j); int a = (k>>24)&0xFF; int r=(k>>16)&0xFF, g=(k>>8)&0xFF, b=k&0xFF;
                    bool ring = (i < inset+1 || j < inset+1 || i >= w-inset-1 || j >= h-inset-1);
                    bool keep = a >= 8 && !ring && MaxDiff(r,g,b, baseC.R, baseC.G, baseC.B) > TOL;
                    int o = j*cs + i*4;
                    if (keep) { cb[o]=(byte)b; cb[o+1]=(byte)g; cb[o+2]=(byte)r; cb[o+3]=(byte)a; }
                }
            Marshal.Copy(cb, 0, cd.Scan0, cb.Length); content.UnlockBits(cd);

            // 2. 画圆角卡片（含柔和投影）
            var rr = new Rectangle(inset, inset, Math.Max(1,w-inset*2), Math.Max(1,h-inset*2));
            int rad = Math.Min(radius, Math.Min(rr.Width, rr.Height) / 2);
            Color fillC = CardFill.A > 0 ? CardFill : Adjust(baseC, fillDelta);
            using (var card = RenderCard(w, h, rr, Math.Max(2, rad),
                        fillC, Adjust(baseC, borderDelta), borderDelta != 0, CardBg))
            using (var g = Graphics.FromImage(card)) {
                // 3. 图标叠回
                g.CompositingMode = CompositingMode.SourceOver;
                g.DrawImage(content, 0, 0);
                Blit(buf, st, W, H, card, x, y, true);
            }
        }
    }

    // 把单元格里有内容的部分重绘成胶囊（用于滑块轨道）
    public static void MakePill(byte[] buf, int st, int W, int H, int x, int y, int w, int h, int delta)
    {
        int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
        for (int j = 0; j < h; j++)
            for (int i = 0; i < w; i++) {
                int k = Px(buf, st, x+i, y+j);
                if (((k>>24)&0xFF) < 8) continue;
                if (i < minX) minX = i; if (i > maxX) maxX = i;
                if (j < minY) minY = j; if (j > maxY) maxY = j;
            }
        if (maxX < 0) return;
        double of;
        Color baseC = Dominant(buf, st, x+minX, y+minY, maxX-minX+1, maxY-minY+1, out of);
        if (of < 0.90) return;                        // 有花样的不做

        int bw = maxX-minX+1, bh = maxY-minY+1;
        using (var pill = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(pill)) {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            var rr = new Rectangle(minX, minY, bw, bh);
            using (var path = RoundPath(rr, Math.Max(1, bh/2)))
            using (var fb = new SolidBrush(Adjust(baseC, delta)))
                g.FillPath(fb, path);
            Blit(buf, st, W, H, pill, x, y, true);
        }
    }

    // ============================================================ 面板细节
    static void DrawPanelDetails(byte[] buf, int st, int W, int H)
    {
        if (W != 1070 || H != 1456) return;
        foreach (int bx in new int[] { 0, 368 }) {
            PaintRect(buf, st, W, H, bx+0,   487, bx+352, 489, CBorder, false);   // 上描边
            PaintRect(buf, st, W, H, bx+0,   730, bx+352, 732, CBorder, false);   // 下描边
            PaintRect(buf, st, W, H, bx+0,   490, bx+2,   729, CBorder, false);   // 左描边
            PaintRect(buf, st, W, H, bx+350, 490, bx+352, 729, CBorder, false);   // 右描边
            PaintRect(buf, st, W, H, bx+3,   490, bx+349, 519, CTitle,  false);   // 标题栏
            PaintRect(buf, st, W, H, bx+3,   521, bx+349, 550, CFiller, false);
            PaintRect(buf, st, W, H, bx+312, 552, bx+349, 729, CFiller, false);
            PaintRect(buf, st, W, H, bx+3,   552, bx+308, 667, CFiller, false);
            PaintRect(buf, st, W, H, bx+3,   669, bx+308, 688, CSeek,   false);   // 进度行
            PaintRect(buf, st, W, H, bx+3,   690, bx+308, 729, CBar,    false);   // 控制条
            // 标题栏顶部高光 + 底部强调线
            PaintRect(buf, st, W, H, bx+3, 490, bx+349, 490, CTitleTop1, false);
            PaintRect(buf, st, W, H, bx+3, 491, bx+349, 491, CTitleTop2, false);
            PaintRect(buf, st, W, H, bx+3, 520, bx+349, 520, CAccentLine, false);
            // 控制条顶部高光
            PaintRect(buf, st, W, H, bx+3, 690, bx+308, 690, CTitleTop1, false);
        }
        PaintRect(buf, st, W, H, 50, 600, 81, 631, CVideo, false);   // 无视频时的画面底
        if (RoundBars) {
            foreach (int bx in new int[] { 0, 368 }) {
                RoundBar(buf, st, W, H, bx+3, 668, 306, 22, 10, CSeek);
                RoundBar(buf, st, W, H, bx+3, 690, 306, 40, BarRadius, CBar);
            }
        }
    }

    // ============================================================ 圆角化清单
    static void RoundBar(byte[] buf, int st, int W, int H, int x, int y, int w, int h, int radius, Color fill)
    {
        int padY = Math.Max(1, h / 16);
        var card = new Rectangle(0, padY, w, h - padY*2);
        int rad = Math.Min(radius, card.Height / 2);
        using (var bmp = RenderCard(w, h, card, Math.Max(2, rad), fill, CDivider, true, CWindowBg))
            Blit(buf, st, W, H, bmp, x, y, true);
    }

    static void Card(byte[] buf, int st, int W, int H, int x, int y, int w, int h, int inset)
    {
        int rad = Math.Min(9, (Math.Min(w, h) - inset*2) / 2);
        MakeCard(buf, st, W, H, x, y, w, h, inset, Math.Max(2, rad), ChipFill, ChipBorder);
    }

    static void Roundify(byte[] buf, int st, int W, int H)
    {
        if (W == 1300 && H == 900) {         // Control.png 控制面板
            Card(buf, st, W, H, 0, 56, 74, 31, 2);          // 页面标签
            Card(buf, st, W, H, 0, 230, 97, 22, 2);
            Card(buf, st, W, H, 407, 230, 41, 22, 2);
            Card(buf, st, W, H, 0, 297, 41, 18, 2);
            Card(buf, st, W, H, 189, 297, 41, 22, 2);
            Card(buf, st, W, H, 326, 565, 82, 14, 2);
            Card(buf, st, W, H, 282, 140, 51, 13, 2);
            Card(buf, st, W, H, 659, 341, 16, 11, 1);
            Card(buf, st, W, H, 659, 357, 16, 12, 1);
            Card(buf, st, W, H, 0, 341, 160, 22, 2);
            for (int i = 0; i < 4; i++) {
                Card(buf, st, W, H, i*217, 389, 51, 22, 2);
                Card(buf, st, W, H, i*217, 477, 51, 22, 2);
            }
            for (int i = 0; i < 3; i++) Card(buf, st, W, H, i*289, 433, 69, 22, 2);
        }
        if (W == 1070 && H == 1456) {
            int[] rows = new int[] { 259, 300, 341 };
            int[] maxG = new int[] { 4, 3, 2 };
            for (int ri = 0; ri < rows.Length; ri++)
                for (int gi = 0; gi < maxG[ri]; gi++) {
                    int gx = gi * 164;
                    for (int s = 0; s < 4; s++)
                        MakeCard(buf, st, W, H, gx + s*41, rows[ri], 41, 40, CardInset, CardRadius, ChipFill, ChipBorder);
                }
            // 滑块轨道 -> 胶囊
            MakePill(buf, st, W, H, 0,   179, 95, 10, 0);
            MakePill(buf, st, W, H, 0,   193, 95, 10, 0);
            MakePill(buf, st, W, H, 0,   207, 95, 10, 0);
            MakePill(buf, st, W, H, 175, 194, 70, 10, 0);
            MakePill(buf, st, W, H, 175, 207, 70, 10, 0);
        }
        if (W == 668 && H == 482) {          // PlayList.png
            // 工具栏上的按钮：底色由 PotPlayer 绘制，透明即可
            for (int s = 0; s < 4; s++)
                MakeCard(buf, st, W, H, s*35, 58, 35, 18, 2, 8, ChipFill, ChipBorder);
            for (int r = 0; r < 4; r++)
                for (int s = 0; s < 4; s++)
                    MakeCard(buf, st, W, H, 160 + s*21, 55 + r*19, 21, 18, 2, 6, ChipFill, ChipBorder);

            // 标签栏由播放列表控件直接绘制，背后没有底色 -> 必须铺底色，否则透明/阴影处会变黑
            Color panelBg = PxColor(buf, st, 518, 48);   // = BkgndColor1
            CardBg = panelBg;
            PaintRect(buf, st, W, H, 0, 141, 339, 171, panelBg, false);
            CardFill = CBar;                             // 让标签页比底色略亮，读作"卡片"
            for (int i = 0; i < 4; i++)                  // 专辑标签页：真实位置 0/85/170/255，宽 84
                MakeCard(buf, st, W, H, i*85, 141, 84, 31, 0, 9, ChipFill, ChipBorder);
            CardFill = Color.Transparent;
            for (int r = 0; r < 3; r++)                  // 专辑标签左右/弹出按钮
                for (int s = 0; s < 4; s++)
                    MakeCard(buf, st, W, H, 253 + s*19, 55 + r*19, 19, 18, 2, 6, ChipFill, ChipBorder);
            CardBg = Color.Transparent;
        }
    }

    // ============================================================ 图像处理
    public static string ProcessImage(string src, string dst)
    {
        string fn = Path.GetFileName(src).ToLowerInvariant();
        // 这些图集本身是「浅色设计」（白底页面 / 浅色滚动条），
        // 浅色主题的色阶会把它们整体反相成黑的，所以只换强调色、保留明暗。
        IdentityNeutral = fn == "brlist.png" || fn == "brtool.png" || fn == "chat.png"
                       || fn == "scrcapture.png" || fn == "window.png";
        using (var orig = new Bitmap(src)) {
            int W = orig.Width, H = orig.Height;
            string pfName = orig.PixelFormat.ToString();
            bool srcAlpha = pfName.IndexOf("Argb", StringComparison.OrdinalIgnoreCase) >= 0
                         || pfName.IndexOf("PArgb", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isBmp = Path.GetExtension(dst).ToLowerInvariant() == ".bmp";

            using (var work = new Bitmap(W, H, PixelFormat.Format32bppArgb)) {
                using (var g = Graphics.FromImage(work)) {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.DrawImage(orig, 0, 0, W, H);
                }
                var rect = new Rectangle(0,0,W,H);
                var data = work.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                int stride = data.Stride, n = Math.Abs(stride)*H;
                byte[] buf = new byte[n];
                Marshal.Copy(data.Scan0, buf, 0, n);

                // 滚动条：轨道色取全图众数；滑块色取「滑块单元格」里的众数（源图里两者往往只差几级灰）
                ScrollBarMode = fn.StartsWith("scrollbar");
                if (ScrollBarMode) {
                    int tx = 53, ty = 0, tw = 9, th = 12;                 // ScrollBar.bmp 的 ThumbMiddle
                    if (fn == "scrollbarpl.bmp") { tx = 126; ty = 5; tw = 20; th = 9; }
                    if (fn.StartsWith("scrollbarch")) { tx = 126; ty = 5; tw = 10; th = 9; }

                    var hist = new Dictionary<int,int>();
                    for (int i = 0; i < n; i += 4) {
                        if (buf[i+3] < 8) continue;
                        int k = (buf[i+2]<<16)|(buf[i+1]<<8)|buf[i];
                        int c; hist.TryGetValue(k, out c); hist[k] = c+1;
                    }
                    int modeK = 0, bestN = -1;
                    foreach (var kv in hist) if (kv.Value > bestN) { bestN = kv.Value; modeK = kv.Key; }
                    ScrollTrackK = modeK;

                    var th2 = new Dictionary<int,int>();
                    for (int y = ty; y < ty+th && y < H; y++)
                        for (int x = tx; x < tx+tw && x < W; x++) {
                            int i = y*stride + x*4;
                            if (buf[i+3] < 8) continue;
                            int k = (buf[i+2]<<16)|(buf[i+1]<<8)|buf[i];
                            int c; th2.TryGetValue(k, out c); th2[k] = c+1;
                        }
                    int thumbK = modeK, bn = -1;
                    foreach (var kv in th2) if (kv.Value > bn) { bn = kv.Value; thumbK = kv.Key; }
                    ScrollThumbK = thumbK;
                }

                int changed = 0;
                for (int y = 0; y < H; y++) {
                    int row = y*stride;
                    for (int x = 0; x < W; x++) {
                        int i = row + x*4;
                        int b = buf[i], g2 = buf[i+1], r = buf[i+2];
                        int a = srcAlpha ? buf[i+3] : 255;
                        if (a == 0) continue;
                        int nr, ng, nb;
                        MapPixel(r, g2, b, out nr, out ng, out nb);
                        if (nr != r || ng != g2 || nb != b) changed++;
                        buf[i]=(byte)nb; buf[i+1]=(byte)ng; buf[i+2]=(byte)nr;
                        if (!srcAlpha) buf[i+3] = 255;
                    }
                }

                DrawPanelDetails(buf, stride, W, H);
                Roundify(buf, stride, W, H);
                if (ScrollBarMode) PaintScrollBar(buf, stride, W, H, fn);

                Marshal.Copy(buf, 0, data.Scan0, n);
                work.UnlockBits(data);

                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                if (!isBmp) work.Save(dst, ImageFormat.Png);
                else {
                    PixelFormat tgt = (pfName == "Format24bppRgb") ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppRgb;
                    using (var b2 = new Bitmap(W, H, tgt))
                    using (var g2 = Graphics.FromImage(b2)) {
                        g2.Clear(Color.White);
                        g2.CompositingMode = CompositingMode.SourceCopy;
                        g2.DrawImage(work, 0, 0, W, H);
                        b2.Save(dst, ImageFormat.Bmp);
                    }
                    // GDI+ 存 BMP 时会把 biSizeImage 留 0，PotPlayer 的解析器不认，会渲染成纯黑。
                    // 这里补成「头部之后的字节数」，与原版皮肤的做法一致。
                    PatchBmpSizeImage(dst);
                }
                return string.Format("{0,-22} {1}x{2} changed={3}", Path.GetFileName(src), W, H, changed);
            }
        }
    }

    // 滚动条的滑块单元格区域（来自 Common.txt 里的 Thumb* 定义）
    static Rectangle[] ScrollThumbRects(string fn)
    {
        if (fn == "scrollbarpl.bmp")
            return new Rectangle[] { new Rectangle(48,0,19,42), new Rectangle(126,0,20,58) };
        if (fn.StartsWith("scrollbarch"))
            return new Rectangle[] { new Rectangle(48,0,9,26),  new Rectangle(126,0,10,58) };
        return new Rectangle[] { new Rectangle(48,0,19,28), new Rectangle(126,0,12,58) };
    }

    // 滚动条整体重绘：铺轨道色 + 只涂滑块单元格
    static void PaintScrollBar(byte[] buf, int st, int W, int H, string fn)
    {
        for (int i = 0; i < buf.Length; i += 4) {
            buf[i] = CScrollTrack.B; buf[i+1] = CScrollTrack.G; buf[i+2] = CScrollTrack.R; buf[i+3] = 255;
        }
        foreach (var r in ScrollThumbRects(fn))
            for (int y = r.Top; y < r.Bottom && y < H; y++)
                for (int x = r.Left; x < r.Right && x < W; x++) {
                    int i = y*st + x*4;
                    buf[i] = CScrollThumb.B; buf[i+1] = CScrollThumb.G; buf[i+2] = CScrollThumb.R; buf[i+3] = 255;
                }
    }

    // 修补 BMP 头里的 biSizeImage（偏移 34），否则 PotPlayer 会把它渲染成黑色
    static void PatchBmpSizeImage(string path)
    {
        try {
            if (!File.Exists(path)) return;
            byte[] b = File.ReadAllBytes(path);
            if (b.Length < 54 || b[0] != 'B' || b[1] != 'M') return;
            uint off  = BitConverter.ToUInt32(b, 10);
            uint size = (uint)Math.Max(0, b.Length - (int)off);
            byte[] sz = BitConverter.GetBytes(size);
            Array.Copy(sz, 0, b, 34, 4);
            File.WriteAllBytes(path, b);
        } catch { }
    }

    // ============================================================ 构建 / 打包
    public static string Build(string srcDir, string outDir)
    {
        if (Directory.Exists(outDir)) Directory.Delete(outDir, true);
        Directory.CreateDirectory(outDir);
        var sb = new StringBuilder();
        sb.AppendLine("theme = " + ThemeName);
        foreach (var f in Directory.GetFiles(srcDir)) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f).ToLowerInvariant();
            string dst = Path.Combine(outDir, name);
            if (ext == ".png" || ext == ".bmp") sb.AppendLine(ProcessImage(f, dst));
            else File.Copy(f, dst, true);
        }
        return sb.ToString();
    }

    // 带标题的构建：title 形如 {$CP936=中文$}{$CP950=繁體$}{$CP0=English$}
    public static string Build(string srcDir, string outDir, string title)
    {
        string r = Build(srcDir, outDir);
        if (!string.IsNullOrEmpty(title)) {
            var bytes = new UTF8Encoding(true).GetBytes(title);   // UTF-8 + BOM
            File.WriteAllBytes(Path.Combine(outDir, "Title.txt"), bytes);
        }
        return r;
    }

    public static string Zip(string dir, string dsfPath)
    {
        if (File.Exists(dsfPath)) File.Delete(dsfPath);
        using (var zip = ZipFile.Open(dsfPath, ZipArchiveMode.Create))
            foreach (var f in Directory.GetFiles(dir))
                zip.CreateEntryFromFile(f, Path.GetFileName(f), CompressionLevel.Optimal);
        return "dsf: " + Path.GetFileName(dsfPath) + "  " + (new FileInfo(dsfPath).Length/1024) + " KB";
    }
}
