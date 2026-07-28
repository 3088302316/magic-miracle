using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace CuteMouseEffects
{
    static class AnimatedCursorRenderer
    {
        const int FrameCount = 12;
        const int CanvasSize = 64;

        public static void WriteWorkingCursor(string shapeId, string output, EffectTheme theme)
        {
            WriteAnimation(output, BuildFrames(shapeId, theme, true), 6, 15);
        }

        public static void WriteWaitCursor(string output, EffectTheme theme)
        {
            WriteAnimation(output, BuildFrames(null, theme, false), 32, 32);
        }

        static List<byte[]> BuildFrames(string shapeId, EffectTheme theme, bool working)
        {
            var frames = new List<byte[]>(FrameCount);
            for (int phase = 0; phase < FrameCount; phase++)
            {
                using (var bitmap = new Bitmap(CanvasSize, CanvasSize, PixelFormat.Format32bppArgb))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        graphics.Clear(Color.Transparent);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        if (working)
                        {
                            CursorShapes.Draw(graphics, shapeId, new RectangleF(2, 12, 44, 44), theme.Shadow, theme.Primary, theme.Secondary, theme.Glow, theme.Core);
                            DrawOrbit(graphics, theme, phase, 52f, 12f, 8.2f, 2.5f);
                        }
                        else
                        {
                            DrawWaitHalo(graphics, theme, phase);
                            DrawOrbit(graphics, theme, phase, 32f, 32f, 14f, 3.8f);
                        }
                    }
                    frames.Add(CursorBytes(bitmap, (ushort)(working ? 6 : 32), (ushort)(working ? 15 : 32)));
                }
            }
            return frames;
        }

        static void DrawOrbit(Graphics graphics, EffectTheme theme, int phase, float cx, float cy, float radius, float dot)
        {
            float backRadius = radius + dot + 2.4f;
            using (var shadow = new SolidBrush(Color.FromArgb(218, theme.Shadow)))
                graphics.FillEllipse(shadow, cx - backRadius, cy - backRadius, backRadius * 2, backRadius * 2);
            using (var inner = new SolidBrush(Color.FromArgb(172, Mix(theme.Shadow, theme.Core, .28f))))
                graphics.FillEllipse(inner, cx - radius - 1.5f, cy - radius - 1.5f, (radius + 1.5f) * 2, (radius + 1.5f) * 2);
            using (var rim = new Pen(Color.FromArgb(205, theme.Glow), 1.2f))
                graphics.DrawEllipse(rim, cx - radius - .8f, cy - radius - .8f, (radius + .8f) * 2, (radius + .8f) * 2);

            for (int i = 0; i < FrameCount; i++)
            {
                int age = (phase - i + FrameCount) % FrameCount;
                float strength = 1f - age / (float)FrameCount;
                double angle = -Math.PI / 2 + i * Math.PI * 2 / FrameCount;
                float x = cx + (float)Math.Cos(angle) * radius;
                float y = cy + (float)Math.Sin(angle) * radius;
                float size = dot * (.52f + strength * .72f);
                Color baseColor = i % 2 == 0 ? theme.Primary : theme.Secondary;
                Color color = Mix(baseColor, theme.Glow, strength * .72f);
                int alpha = Math.Max(56, Math.Min(255, (int)(68 + strength * 187)));
                using (var brush = new SolidBrush(Color.FromArgb(alpha, color)))
                    graphics.FillEllipse(brush, x - size / 2, y - size / 2, size, size);
            }

            int head = phase % FrameCount;
            double headAngle = -Math.PI / 2 + head * Math.PI * 2 / FrameCount;
            float hx = cx + (float)Math.Cos(headAngle) * radius;
            float hy = cy + (float)Math.Sin(headAngle) * radius;
            using (var pen = new Pen(Color.FromArgb(245, theme.Glow), 1.15f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                graphics.DrawLine(pen, hx - dot, hy, hx + dot, hy);
                graphics.DrawLine(pen, hx, hy - dot, hx, hy + dot);
            }
        }

        static void DrawWaitHalo(Graphics graphics, EffectTheme theme, int phase)
        {
            float pulse = (float)(.5 + .5 * Math.Sin(phase * Math.PI * 2 / FrameCount));
            using (var pen = new Pen(Color.FromArgb((int)(92 + pulse * 80), theme.Secondary), 2f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                graphics.DrawArc(pen, 10, 10, 44, 44, phase * 30 - 35, 105);
            }
            using (var pen = new Pen(Color.FromArgb((int)(74 + (1 - pulse) * 72), theme.Primary), 1.45f))
            {
                pen.StartCap = pen.EndCap = LineCap.Round;
                graphics.DrawArc(pen, 15, 15, 34, 34, -phase * 30 + 145, 82);
            }
            using (var glow = new SolidBrush(Color.FromArgb(210, theme.Glow)))
                graphics.FillEllipse(glow, 30.2f, 30.2f, 3.6f, 3.6f);
        }

        static Color Mix(Color a, Color b, float amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * amount),
                (int)(a.G + (b.G - a.G) * amount),
                (int)(a.B + (b.B - a.B) * amount));
        }

        static byte[] CursorBytes(Bitmap bitmap, ushort hotspotX, ushort hotspotY)
        {
            byte[] png;
            using (var image = new MemoryStream())
            {
                bitmap.Save(image, ImageFormat.Png);
                png = image.ToArray();
            }
            using (var cursor = new MemoryStream())
            using (var writer = new BinaryWriter(cursor))
            {
                writer.Write((ushort)0);
                writer.Write((ushort)2);
                writer.Write((ushort)1);
                writer.Write((byte)CanvasSize);
                writer.Write((byte)CanvasSize);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write(hotspotX);
                writer.Write(hotspotY);
                writer.Write((uint)png.Length);
                writer.Write((uint)22);
                writer.Write(png);
                return cursor.ToArray();
            }
        }

        static void WriteAnimation(string output, IList<byte[]> frames, uint hotspotX, uint hotspotY)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                FourCC(writer, "RIFF");
                writer.Write((uint)0);
                FourCC(writer, "ACON");

                FourCC(writer, "anih");
                writer.Write((uint)36);
                writer.Write((uint)36);
                writer.Write((uint)frames.Count);
                writer.Write((uint)frames.Count);
                writer.Write((uint)CanvasSize);
                writer.Write((uint)CanvasSize);
                writer.Write((uint)32);
                writer.Write((uint)1);
                writer.Write((uint)4);
                writer.Write((uint)1);

                FourCC(writer, "rate");
                writer.Write((uint)(frames.Count * 4));
                for (int i = 0; i < frames.Count; i++) writer.Write((uint)4);

                using (var listStream = new MemoryStream())
                using (var listWriter = new BinaryWriter(listStream))
                {
                    FourCC(listWriter, "fram");
                    foreach (byte[] frame in frames)
                    {
                        FourCC(listWriter, "icon");
                        listWriter.Write((uint)frame.Length);
                        listWriter.Write(frame);
                        if ((frame.Length & 1) != 0) listWriter.Write((byte)0);
                    }
                    FourCC(writer, "LIST");
                    writer.Write((uint)listStream.Length);
                    writer.Write(listStream.ToArray());
                    if ((listStream.Length & 1) != 0) writer.Write((byte)0);
                }

                stream.Position = 4;
                writer.Write((uint)(stream.Length - 8));
                File.WriteAllBytes(output, stream.ToArray());
            }
        }

        static void FourCC(BinaryWriter writer, string value)
        {
            writer.Write(Encoding.ASCII.GetBytes(value));
        }
    }
}
