using SkiaSharp;
using FollowerProcessing;

namespace TreeProcessing
{
    /// <summary>
    /// Класс, отрисовывающий дерево возможных решений.
    /// </summary>
    public class TreeVisualizer
    {

        private const int ImageSize = 100; 
        private const int HorizontalSpacing = 110; 
        private const int VerticalSpacing = 120; 
        private const int TextPadding = 20;
        [Obsolete]
        private readonly SKPaint _textPaint = new()
        {
            Color = SKColors.BlueViolet,
            TextSize = 22,
            Typeface = SKTypeface.FromFamilyName("Times New Roman"),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center // Текст центрируется по горизонтали
        };

        private readonly SKPaint _linePaint = new()
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        /// <summary>
        /// Основной метод для визуализации дерева последователей.
        /// </summary>
        /// <param name="followers">Словарь всех последователей</param>
        /// <param name="rootId">ID начального последователя</param>
        /// <param name="iconsFolderPath">Путь к папке с иконками последователей.</param>
        public void VisualizeTree(Dictionary<string, Follower> followers, string rootId, string iconsFolderPath)
        {
            Dictionary<string, SKPoint> positions = [];

            CalculatePositions(followers, rootId, positions, new SKPoint(100, 50));

            float maxX = 0; float maxY = 0;
            foreach (SKPoint p in positions.Values)
            {
                maxX = p.X < maxX ? maxX : p.X;
                maxY = p.Y < maxY ? maxY : p.Y;
            }

            using SKSurface surface = SKSurface.Create(new SKImageInfo((int)maxX + ImageSize + 50, (int)maxY + ImageSize + 50));
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            // Отрисовываем связи между узлами
            DrawConnections(canvas, followers, positions);
            // Отрисовываем полследователей
            DrawNodes(canvas, followers, positions, iconsFolderPath);
            SaveToFile(surface);
        }

        /// <summary>
        /// Рекурсивно рассчитывает позиции для всех узлов дерева.
        /// </summary>
        /// <param name="followers">Словарь всех последователей.</param>
        /// <param name="currentId">ID текущего узла.</param>
        /// <param name="positions">Словарь для хранения позиций узлов.</param>
        /// <param name="currentPosition">Текущая позиция узла на холсте.</param>
        private void CalculatePositions(Dictionary<string, Follower> followers,
                                      string currentId,
                                      Dictionary<string, SKPoint> positions,
                                      SKPoint currentPosition)
        {
            // Если текущий узел не найден в словаре, сохраняем его позицию и завершаем рекурсию
            if (!followers.ContainsKey(currentId))
            {
                positions[currentId] = currentPosition;
                return;
            }

            positions[currentId] = currentPosition;
            Follower follower = followers[currentId];

            // Если у узла нет дочерних элементов, завершаем рекурсию
            int childCount = follower.XTriggers.Count;
            if (childCount == 0)
            {
                return;
            }

            float startX = currentPosition.X + (childCount * 35);
            float y = currentPosition.Y + VerticalSpacing + (childCount * 20);

            int index = 0;
            foreach (string child in follower.XTriggers.Values)
            {
                float x = startX + (HorizontalSpacing * (index < 4 ? index * 3 : 4));
                CalculatePositions(followers, child, positions, new SKPoint(x, y));
                index++;
            }
        }

        /// <summary>
        /// Отрисовывает узлы (иконки и текст) на холсте.
        /// </summary>
        /// <param name="canvas">Холст для отрисовки.</param>
        /// <param name="followers">Словарь всех последователей.</param>
        /// <param name="positions">Словарь с позициями узлов.</param>
        /// <param name="iconsFolderPath">Путь к папке с иконками.</param>
        [Obsolete]
        private void DrawNodes(SKCanvas canvas,
                             Dictionary<string, Follower> followers,
                             Dictionary<string, SKPoint> positions,
                             string iconsFolderPath)
        {
            foreach (KeyValuePair<string, SKPoint> entry in positions)
            {
                string imagePath;
                if (!followers.ContainsKey(entry.Key))
                {
                    imagePath = @"default.png";
                    SKPoint pos = entry.Value;

                    using SKImage image = SKImage.FromEncodedData(imagePath);
                    if (image != null)
                    {
                        // Отрисовываем иконку
                        canvas.DrawImage(image, new SKRect(pos.X - (ImageSize / 2), pos.Y, pos.X + (ImageSize / 2), pos.Y + ImageSize));
                    }

                    canvas.DrawText(entry.Key, pos.X, pos.Y + ImageSize + TextPadding, _textPaint);
                }
                else
                {
                    // Если узел найден, используем его иконку
                    Follower follower = followers[entry.Key];
                    SKPoint pos = entry.Value;

                    imagePath = follower.GetIconPath(iconsFolderPath);
                    using SKImage image = SKImage.FromEncodedData(imagePath);
                    if (image != null)
                    {
                        // Отрисовываем иконку
                        canvas.DrawImage(image, new SKRect(pos.X - (ImageSize / 2), pos.Y, pos.X + (ImageSize / 2), pos.Y + ImageSize));
                    }

                    // Отрисовываем текст под иконкой
                    canvas.DrawText(follower.GetField("label"), pos.X, pos.Y + ImageSize + TextPadding, _textPaint);
                }
            }
        }

        /// <summary>
        /// Отрисовывает связи между узлами.
        /// </summary>
        /// <param name="canvas">Холст для отрисовки.</param>
        /// <param name="followers">Словарь всех последователей.</param>
        /// <param name="positions">Словарь с позициями узлов.</param>
        private void DrawConnections(SKCanvas canvas, Dictionary<string, Follower> followers, Dictionary<string, SKPoint> positions)
        {
            foreach (KeyValuePair<string, SKPoint> entry in positions)
            {
                if (!followers.ContainsKey(entry.Key))
                {
                    continue; // Пропускаем узлы, которые не являются последователями
                }
                Follower follower = followers[entry.Key];
                SKPoint startPos = entry.Value;

                // Отрисовываем линии к каждому дочернему узлу
                foreach (string childId in follower.XTriggers.Values)
                {
                    SKPoint endPos = positions[childId];
                    canvas.DrawLine(startPos.X,
                                  startPos.Y + ImageSize,
                                  endPos.X,
                                  endPos.Y - TextPadding,
                                  _linePaint);
                }
            }
        }

        /// <summary>
        /// Сохраняет результат визуализации в файл.
        /// </summary>
        /// <param name="surface">Поверхность SkiaSharp, содержащая изображение.</param>
        /// <param name="path">Путь для сохранения файла (по умолчанию: "../../../../tree_output.png").</param>
        private void SaveToFile(SKSurface surface, string path = @"../../../../tree_output.png")
        {
            using SKImage image = surface.Snapshot();
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100); // Кодируем в PNG
            using FileStream stream = File.OpenWrite(path);
            data.SaveTo(stream); // Сохраняем в файл
        }
    }
}