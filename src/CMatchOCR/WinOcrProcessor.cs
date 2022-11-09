using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Rectangle = Windows.Foundation.Rect;
using Point = Windows.Foundation.Point;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace CMatchOCR
{
    /// <summary>
    /// Processes text from Windows OCR
    /// </summary>
    public class WinOcrProcessor
    {
        private const int MinimumWordSize = 3;
        private readonly OcrEngine ocrEngine;
        
        /// <summary>
        /// Initialize the OCR engine
        /// </summary>
        public WinOcrProcessor()
        {
            // initialize the OCR engine using the User Profile Languages
            ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();

            if (ocrEngine == null)
                throw new Exception("Could not create OcrEngine from User Profile Languages.");
        }

        /// <summary>
        /// Initialize the OCR engine with the specified language
        /// </summary>
        /// <param name="language">The language to use for the OCR engine</param>
        public WinOcrProcessor(Windows.Globalization.Language language)
        {
            // initialize the OCR engine using the User Profile Languages
            ocrEngine = OcrEngine.TryCreateFromLanguage(language);

            if (ocrEngine == null)
                throw new Exception("Could not create OcrEngine, the specified language could not be resolved.");
        }

        /// <summary>
        /// Approximate the location of a word
        /// </summary>
        /// <param name="word">The word to extract the point from</param>
        /// <returns>The center point of the word</returns>
        public static Point ExtractPoint(Word word)
        {
            var rect = word.OcrWord.BoundingRect;
            var fullText = word.OcrWord.Text;
            var preText = fullText.Substring(0, word.Index);

            // approximate the size and location of the word
            var averageCharacterWidth = rect.Width / fullText.Length;
            var wordRelativePositionX = averageCharacterWidth * preText.Length;
            var wordWidth = averageCharacterWidth * word.Text.Length;

            return new Point(rect.X + wordRelativePositionX + wordWidth / 2d, rect.Y + rect.Height / 2d);
        }

        /// <summary>
        /// Extract the bounding rectangle for a word
        /// </summary>
        /// <param name="word">The word to extract the rectangle from</param>
        /// <returns>The extracted rectangle for the word</returns>
        public static Rectangle ExtractRect(Word word)
        {            
            var rect = word.OcrWord.BoundingRect;
            var fullText = word.OcrWord.Text;
            var preText = fullText.Substring(0, word.Index);

            // approximate the size and location of the word
            var averageCharacterWidth = rect.Width / fullText.Length;
            var wordRelativePositionX = averageCharacterWidth * preText.Length;
            var wordWidth = averageCharacterWidth * word.Text.Length;

            return new Rectangle(rect.X + wordRelativePositionX, rect.Y, wordWidth, rect.Height); ;
        }

        /// <summary>
        /// Extract the text from a SoftwareBitmap
        /// </summary>
        /// <param name="softwareBitmap">The image to extract the text from</param>
        /// <returns>The extracted text in the form of a string</returns>
        public async Task<string> ExtractText(SoftwareBitmap softwareBitmap)
        {
            // scan the image for text
            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

            return ocrResult.Text;
        }

        /// <summary>
        /// Extract the text from a SoftwareBitmap
        /// </summary>
        /// <param name="softwareBitmap">The image to extract the text from</param>
        /// <returns>A collection of the extracted words</returns>
        public async Task<IEnumerable<Word>> ExtractWords(SoftwareBitmap softwareBitmap)
        {            
            // scan the image for text
            var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

            // Console.WriteLine("Extracted text: " + ocrResult.Text);

            // parse the OCR result into a list of words
            var maxNumberOfWords = ocrResult.Text.Length / MinimumWordSize;
            var words = new List<Word>(maxNumberOfWords);
            foreach (var line in ocrResult.Lines)
            {
                foreach (var word in line.Words)
                    ParseOcrWord(word, ref words);
            }

            return words;
        }

        /// <summary>
        /// Parse a OcrWord into a list of words
        /// </summary>
        /// <param name="ocrWord">OcrWord to be parsed</param>
        /// <param name="words">The output list of words</param>
        private static void ParseOcrWord(OcrWord ocrWord, ref List<Word> words)
        {
            // an OcrWord may contain multiple words
            var text = ocrWord.Text;
            var stringBuilder = new StringBuilder();
            var index = 0;
            for (var i = 0; i < text.Length; i++)
            { 
                var c = text[i];
                if (char.IsLower(c) || char.IsDigit(c) || (char.IsUpper(c) && stringBuilder.Length == 0))
                {
                    stringBuilder.Append(c);
                    continue;
                }
                
                if (stringBuilder.Length >= MinimumWordSize)
                    words.Add(new Word(stringBuilder.ToString(), index, ocrWord));

                stringBuilder.Clear();
                if (char.IsUpper(c))
                {
                    stringBuilder.Append(c);
                    index = i;
                }
                else
                    index = i + 1;
            }
            if (stringBuilder.Length >= MinimumWordSize)
                words.Add(new Word(stringBuilder.ToString(), index, ocrWord));
        }
    }
}
