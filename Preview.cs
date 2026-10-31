using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

public static class Preview
{
    static void Stretch(Graphics g, Bitmap a, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
    {
        g.DrawImage(a, new Rectangle(dx, dy, dw, dh), new Rectangle(sx, sy, sw, sh), GraphicsUnit.Pixel);
    }
    static Color At(Bitmap a, int x, int y) { return a.GetPixel(x, y); }

    // 按 PotPlayer 布局文件的单元格规则合成主窗口
    static GraphicsPath RPath(Rectangle r, int rad)
    {
        var p = new GraphicsPath(); int d = Math.Max(1, rad*2);
        if (d > r.Width) d = r.Width; if (d > r.Height) d = r.Height;
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right-d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom-d, d, d, 90, 90);
        p.CloseFigure(); return p;
    }

    public static void DrawWindow(Graphics g, Bitmap a, int? panelColorOverride, int x, int y, int w, int h, string title, bool dwm)
    {
        int TOP = dwm ? 0 : 3, TITLE = 31, SEEK = 22, BTN = 40, BOT = dwm ? 0 : 3;
        int clientY = y + TOP + TITLE;
        int clientH = h - TOP - TITLE - BOT;
        int btnY = y + h - BOT - BTN;
        int seekY = btnY - SEEK;
        int videoH = seekY - clientY;

        Color panel  = At(a, 320, 600);
        Color textHi = At(a, 864, 494);
        Color textLo = At(a, 875, 528);

        int inX = dwm ? 0 : 3, inW = dwm ? w : w - 6;
        using (var b = new SolidBrush(panel)) g.FillRectangle(b, x + inX, clientY, inW, clientH);

        if (!dwm) {
            // 自绘边框（UseWindowFrame=0）
            Stretch(g, a, 0, 487, 353, 3, x, y, w, TOP);
            Stretch(g, a, 0, 730, 353, 3, x, y + h - BOT, w, BOT);
            Stretch(g, a, 0, 490, 3, 240, x, y + TOP, 3, h - TOP - BOT);
            Stretch(g, a, 350, 490, 3, 240, x + w - 3, y + TOP, 3, h - TOP - BOT);
        }
        // 标题栏
        Stretch(g, a, 3, 490, 347, TITLE, x + inX, y + TOP, inW, TITLE);

        // 标题栏左侧菜单按钮（PotPlayer 徽标）
        Stretch(g, a, 0, 43, 89, 30, x + inX, y + TOP, 89, 30);

        // 标题栏右侧窗口按钮
        int bx = x + w - (dwm ? 0 : 3) - 10;
        Stretch(g, a, 260, 20, 14, 13, bx - 14, y + TOP + 9, 14, 13); bx -= 14 + 10;
        Stretch(g, a, 65, 20, 14, 13, bx - 14, y + TOP + 9, 14, 13);  bx -= 14;
        Stretch(g, a, 0, 20, 14, 13, bx - 14, y + TOP + 9, 14, 13);   bx -= 14 + 10;
        Stretch(g, a, 550, 68, 14, 13, bx - 14, y + TOP + 9, 14, 13);

        // 标题文字
        using (var f = new Font("Microsoft YaHei UI", 8.5f, FontStyle.Regular))
        using (var tb = new SolidBrush(textHi))
        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            g.DrawString(title, f, tb, new RectangleF(x + 100, y + TOP, w - 300, TITLE), sf);

        // 视频画面（渐变占位 + 强调色播放圆）
        var vr = new Rectangle(x + inX, clientY, inW, videoH);
        using (var lg = new LinearGradientBrush(vr, Color.FromArgb(255, 28, 38, 52), Color.FromArgb(255, 8, 12, 18), 65f))
            g.FillRectangle(lg, vr);
        Color accent = At(a, 864, 99);
        int cx = vr.X + vr.Width / 2, cy = vr.Y + vr.Height / 2;
        using (var ab = new SolidBrush(Color.FromArgb(70, accent)))
            g.FillEllipse(ab, cx - 34, cy - 34, 68, 68);
        using (var ab = new SolidBrush(Color.FromArgb(235, accent)))
            g.FillPolygon(ab, new Point[] {
                new Point(cx - 10, cy - 17), new Point(cx + 20, cy), new Point(cx - 10, cy + 17) });

        // 进度条行
        Stretch(g, a, 3, 668, 306, SEEK, x + 10, seekY, w - 20, SEEK);
        int tbY = seekY + 6, tbX = x + 20, tbW = w - 40;
        Stretch(g, a, 0, 193, 95, 10, tbX, tbY, tbW, 10);                       // 轨道
        int played = (int)(tbW * 0.34);
        Stretch(g, a, 0, 179, 95, 10, tbX, tbY, played, 10);                    // 已播放
        Stretch(g, a, 1, 239, 11, 10, tbX + played - 5, tbY, 11, 10);           // 滑块
        using (var f = new Font("Tahoma", 7f))
        using (var tb2 = new SolidBrush(textLo))
        {
            g.DrawString("00:12:34", f, tb2, tbX + tbW + 8, tbY - 1);
            g.DrawString("01:23:45", f, tb2, tbX + tbW + 62, tbY - 1);
        }

        // 控制按钮行
        Stretch(g, a, 3, 690, 306, BTN, x + 10, btnY, w - 20, BTN);
        int bxx = x + 12;
        Stretch(g, a, 164, 300, 41, 40, bxx, btnY, 41, 40);          bxx += 41;   // 上一段
        Stretch(g, a, 0,   259, 41, 40, bxx, btnY, 41, 40);          bxx += 41;   // 播放
        Stretch(g, a, 0,   300, 41, 40, bxx, btnY, 41, 40);          bxx += 41;   // 停止
        Stretch(g, a, 328, 300, 41, 40, bxx, btnY, 41, 40);          bxx += 41;   // 下一段
        Stretch(g, a, 0,   341, 41, 40, bxx, btnY, 41, 40);          bxx += 41;   // 打开
        using (var f = new Font("Tahoma", 7.5f))
        using (var tb3 = new SolidBrush(textHi))
        {
            g.DrawString("00:12:34", f, tb3, x + 12 + 41 * 5 + 8, btnY + 15);
            using (var tb4 = new SolidBrush(textLo)) g.DrawString("/  01:23:45", f, tb4, x + 12 + 41 * 5 + 52, btnY + 15);
        }
        int rx = x + w - 12;
        Stretch(g, a, 303, 391, 23, 22, rx - 23, btnY + 9, 23, 22); rx -= 23 + 8;
        Stretch(g, a, 202, 391, 23, 22, rx - 23, btnY + 9, 23, 22); rx -= 23 + 8;
        Stretch(g, a, 0, 391, 23, 22, rx - 23, btnY + 9, 23, 22);   rx -= 23 + 8;
        Stretch(g, a, 101, 455, 23, 22, rx - 23, btnY + 9, 23, 22); rx -= 23 + 8;
        Stretch(g, a, 0, 455, 23, 22, rx - 23, btnY + 9, 23, 22);
    }

    public static string Render(string origMain, string newMain, string outPath, string newLabel, string subtitle, string footer, bool dwm)
    {
        const int W = 1240, H = 600;
        using (var cv = new Bitmap(W, H, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(cv))
        using (var oa = new Bitmap(origMain))
        using (var na = new Bitmap(newMain))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            using (var bg = new LinearGradientBrush(new Rectangle(0, 0, W, H),
                       Color.FromArgb(255, 17, 22, 29), Color.FromArgb(255, 6, 8, 12), 90f))
                g.FillRectangle(bg, 0, 0, W, H);

            Color accent = At(na, 864, 99);
            using (var ab = new SolidBrush(accent)) g.FillRectangle(ab, 40, 40, 4, 40);
            using (var f1 = new Font("Microsoft YaHei UI", 19f, FontStyle.Bold))
            using (var wb = new SolidBrush(Color.FromArgb(255, 238, 243, 250)))
                g.DrawString("PotPlayer 面板风 · 圆润版", f1, wb, 56, 32);
            using (var f2 = new Font("Microsoft YaHei UI", 9.5f))
            using (var gb = new SolidBrush(Color.FromArgb(255, 130, 146, 165)))
                g.DrawString(subtitle, f2, gb, 58, 68);
            using (var pen = new Pen(Color.FromArgb(60, accent), 1))
                g.DrawLine(pen, 40, 100, W - 40, 100);

            using (var f3 = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold))
            using (var lb = new SolidBrush(Color.FromArgb(255, 150, 162, 178)))
            using (var f4 = new Font("Microsoft YaHei UI", 10f, FontStyle.Bold))
            using (var ab2 = new SolidBrush(accent))
            {
                g.DrawString("原版 Default", f3, lb, 44, 122);
                g.DrawString(newLabel, f4, ab2, 644, 122);
            }
            DrawWindow(g, oa, null, 40, 152, 560, 330, "示例视频.mp4 - PotPlayer", false);
            if (!dwm) {
                DrawWindow(g, na, null, 640, 152, 560, 330, "示例视频.mp4 - PotPlayer", false);
            } else {
                // 模拟 Win11 DWM 圆角窗口：无自绘边框 + 8px 圆角裁剪 + 系统投影
                var wr = new Rectangle(640, 152, 560, 330);
                using (var tb = new Bitmap(wr.Width, wr.Height, PixelFormat.Format32bppArgb))
                using (var tg = Graphics.FromImage(tb)) {
                    tg.SmoothingMode = SmoothingMode.AntiAlias;
                    tg.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    DrawWindow(tg, na, null, 0, 0, wr.Width, wr.Height, "示例视频.mp4 - PotPlayer", true);
                    using (var sp = RPath(new Rectangle(wr.X - 3, wr.Y + 3, wr.Width + 6, wr.Height + 6), 12))
                    using (var sb2 = new SolidBrush(Color.FromArgb(80, 0, 0, 0)))
                        g.FillPath(sb2, sp);
                    using (var path = RPath(wr, 8)) {
                        g.SetClip(path);
                        g.DrawImage(tb, wr.X, wr.Y);
                        g.ResetClip();
                    }
                    using (var pen = new Pen(Color.FromArgb(38, 0, 0, 0), 1f))
                    using (var path = RPath(wr, 8)) g.DrawPath(pen, path);
                }
            }

            using (var f5 = new Font("Microsoft YaHei UI", 8.5f))
            using (var fb = new SolidBrush(Color.FromArgb(255, 105, 118, 134)))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString(footer, f5, fb, new RectangleF(0, H - 44, W, 24), sf);

            cv.Save(outPath, ImageFormat.Png);
        }
        return outPath;
    }
}