using System;
using Rectangle = Windows.Foundation.Rect;
using Windows.Graphics.Imaging;
using FuzzySharp.Extractor;
using System.Collections.Generic;
using Process = FuzzySharp.Process;
using System.Threading.Tasks;

namespace CMatchOCR
{
    /// <summary>
    /// A collection of methods for finding the position of text on the screen
    /// </summary>
    public class ScreenTextFinder
    {
        private readonly WinOcrProcessor screenProcessor;
        private readonly Func<Word, string> wordProcessor = Word.Process;

        /// <summary>
        /// Initializes the word finder for the specified display and default system language
        /// </summary>
        public ScreenTextFinder()
        {
            screenProcessor = new WinOcrProcessor();
        }

        /// <summary>
        /// Initializes the word finder for the specified display and language
        /// </summary>
        /// <param name="language">The language that the word is from</param>
        public ScreenTextFinder(Windows.Globalization.Language language)
        {
            screenProcessor = new WinOcrProcessor(language);

        }

        /// <summary>
        /// Try to get the bounding rectangles for all the words matching the specified text on the screen 
        /// </summary>
        /// <param name="text">The word to search for</param>
        /// <param name="x">The x-coordinate on the screen in pixels</param>
        /// <param name="y">The y-coordinate on the screen in pixels</param>
        /// <param name="size">The size of the search area (a square area of side length size centered at the
        ///                     coordinates (x, y))</param>
        /// <returns>Returns the bounding rectangles for all the words matching the specified text</returns>
        public async Task<IEnumerable<Rectangle>> FindAll(string text, double x, double y, int size)
        {
            var screenRect = new System.Drawing.Rectangle((int)(x - size / 2f), (int)(y - size / 2f),
                size * 2, size * 2);
            return await FindAll(text, screenRect);
        }

        /// <summary>
        /// Try to get the bounding rectangles for all the words matching the specified text on the screen 
        /// </summary>
        /// <param name="text">The word to search for</param>
        /// <param name="screenRect">The area on the screen to search</param> 
        /// <returns>Returns the bounding rectangles for all the words matching the specified text</returns>
        public async Task<IEnumerable<Rectangle>> FindAll(string text, System.Drawing.Rectangle screenRect)
        {
            // grab the screen at the area defined by the screenRect
            SoftwareBitmap softwareBitmap = await ScreenshotUtility.ScreenshotRaw(screenRect);

            // extract the words from the screen area
            var words = screenProcessor.ExtractWords(softwareBitmap);

            // find the words most similar to the specified text 
            var extractedResults = Process.ExtractAll(new Word(text, 0, null),
                words.Result, wordProcessor, null, 80);

            var boundingRects = new List<Rectangle>(((List<Word>)words.Result).Count);
            foreach (var extractedResult in extractedResults)
            {
                var extractedRect = WinOcrProcessor.ExtractRect(extractedResult.Value);
                var wordScreenRect = new Rectangle(screenRect.X + extractedRect.X, screenRect.Y + extractedRect.Y,
                    extractedRect.Width, extractedRect.Height);
                boundingRects.Add(wordScreenRect);
            }

            return boundingRects;
        }

        /// <summary>
        /// Try to get the bounding rectangle for the word on the screen closest to the specified coordinates
        /// </summary>
        /// <param name="text">The word to search for</param>
        /// <param name="x">The x-coordinate on the screen in pixels</param>
        /// <param name="y">The y-coordinate on the screen in pixels</param>
        /// <param name="size">The size of the search area (a square area of side length size centered at the
        ///                     coordinates (x, y))</param>
        /// <returns>Returns the bounding rectangle of the screen text if successful, else returns an empty rectangle</returns>
        public async Task<Rectangle> TryFindClosest(string text, double x, double y, int size)
        {
            var screenRect = new System.Drawing.Rectangle((int)(x - size / 2f), (int)(y - size / 2f),
                size, size);
            return await TryFindClosest(text, screenRect);
        }

        /// <summary>
        /// Try to get the bounding rectangle for the word on the screen closest to the specified coordinates
        /// </summary>
        /// <param name="text">The word to search for</param>
        /// <param name="screenRect">The area on the screen to search</param> 
        /// <returns>Returns the bounding rectangle of the screen text if successful, else returns an empty rectangle</returns>
        public async Task<Rectangle> TryFindClosest(string text, System.Drawing.Rectangle screenRect)
        {
            // grab the screen at the area defined by the screenRect
            SoftwareBitmap softwareBitmap = await ScreenshotUtility.ScreenshotRaw(screenRect);

            // extract the words from the screen area
            var words = screenProcessor.ExtractWords(softwareBitmap);

            // find the words most similar to the specified text 
            var extractedResults = Process.ExtractTop(
                new Word(text, 0, null), words.Result, wordProcessor, null, 5, 80);

            // find the word closest to the given position 
            //     (in this case the position closest to the center of the screen shot)
            if (TryGetWordClosestToPosition(screenRect.Width / 2d, screenRect.Height / 2d,
                    extractedResults, out var closestWord))
            {
                // get the relative bounding rect
                var extractedRect = WinOcrProcessor.ExtractRect(closestWord);
                // calculate the screen rect
                return new Rectangle(screenRect.X + extractedRect.X, screenRect.Y + extractedRect.Y,
                    extractedRect.Width, extractedRect.Height);
            }

            return Rectangle.Empty;
        }

        /// <summary>
        /// Try to get the word that is closest to the specified position
        /// </summary>
        /// <param name="x">The x-coordinate of the position</param>
        /// <param name="y">The y-coordinate of the position</param>
        /// <param name="wordsResultEnumerable">The list of results containing the words to search</param>
        /// <param name="closestWord">The output for the word closest to the specified position</param>
        /// <returns>True if a word is successfully found</returns>
        private bool TryGetWordClosestToPosition(double x, double y,
            IEnumerable<ExtractedResult<Word>> wordsResultEnumerable, out Word closestWord)
        {
            ExtractedResult<Word> nearestResult = null;
            double smallestSquareDistance = int.MaxValue;
            foreach (var currentWord in wordsResultEnumerable)
            {
                // Get the center point of the current word
                var wordPosition = WinOcrProcessor.ExtractPoint(currentWord.Value);
                // Calculate the square distance of the current word to the given position
                var squareDistance = (wordPosition.X - x) * (wordPosition.X - x) +
                                     (wordPosition.Y - y) * (wordPosition.Y - y);
                // Check to see if this word is closer to the position than any previous words
                if (squareDistance >= smallestSquareDistance) continue;
                smallestSquareDistance = squareDistance;
                nearestResult = currentWord;
            }

            if (nearestResult == null)
            {
                closestWord = new Word();
                return false;
            }

            closestWord = nearestResult.Value;
            return true;
        }
    }
}