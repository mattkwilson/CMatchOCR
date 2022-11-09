using Windows.Media.Ocr;

namespace CMatchOCR
{
    /// <summary>
    /// A single word parsed from an OcrWord
    /// </summary>
    public readonly struct Word
    {
        /// <summary>
        /// The text representation of the word
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// The position of the text within the OcrWord
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Reference to the OcrWord 
        /// </summary>
        public readonly OcrWord OcrWord;

        /// <summary>
        /// Create a new word
        /// </summary>
        /// <param name="text">The text representation of the word</param>
        /// <param name="index">The position of the text within the OcrWord</param>
        /// <param name="ocrWord">Reference to the OcrWord</param>
        public Word(string text, int index, OcrWord ocrWord)
        {
            Text = text;
            Index = index;
            OcrWord = ocrWord;
        }

        /// <summary>
        /// Process a word to get the text
        /// </summary>
        /// <param name="word">The word to process</param>
        /// <returns>The text representation of the word</returns>
        public static string Process(Word word)
        {
            return word.Text.ToLower();
        }
    }
}