using MeCab;

namespace BertJapaneseTokenizer
{
    public class BertJapaneseTokenizer
    {
        public Dictionary<string, int>? Vocab
        {
            get
            {
                return vocab;
            }
        }
        public MeCabTagger? Tagger
        {
            get
            {
                return tagger;
            }
        }
        public const string unkToken = "[UNK]";

        private readonly Dictionary<string, int> vocab = [];
        private readonly MeCabTagger? tagger;
        private const int maxInputCharsPerWord = 100;

        private BertJapaneseTokenizer()
        { }
        public BertJapaneseTokenizer(string dictPath, string vocabPath)
        {
            // Load the vocabulary into a dictionary
            using (var reader = File.OpenText(vocabPath))
            {
                string line;
                int id = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    // Assuming the vocab.txt file contains one token per line
                    vocab[line] = id++;
                }
            }

            var parameter = new MeCabParam(dictPath);
            tagger = MeCabTagger.Create(parameter);
        }

        public int[] EncodePlus(string sentence, bool addSpecialTokens = true)
        {
            int[] result = [];

            sentence = doNormalize(sentence);
            string[] tokens = doWordTokenize(sentence);
            string[] splitTokens = doSubwordTokenize(tokens);
            result = doConvertTokensToIds(splitTokens);
            result = appendSpecialTokens(result);

            return result;
        }

        private int[] appendSpecialTokens(int[] result)
        {
            List<int> results = [];
            results.Add(vocab["[CLS]"]);
            results.AddRange(result);
            results.Add(vocab["[SEP]"]);
            return results.ToArray();
        }

        private string doNormalize(string sentence)
        {
            return sentence.Normalize(System.Text.NormalizationForm.FormKC);
        }

        private string[] doWordTokenize(string sentence)
        {
            List<string> result = [];
            var tokens = Tagger.ParseToNodes(sentence);

            foreach (var node in tokens)
            {
                if (node.CharType > 0)
                {
                    result.Add(node.Surface);
                }
            }
            return result.ToArray();
        }

        private string[] doSubwordTokenize(string[] tokens)
        {
            List<string> result = [];

            foreach (var token in tokens)
            {
                var subTokens = WordPieceTokenize(token);
                result.AddRange(subTokens);
            }

            return result.ToArray();
        }

        private int[] doConvertTokensToIds(string[] splitTokens)
        {
            List<int> results = [];

            foreach (var token in splitTokens)
            {
                if (vocab.TryGetValue(token, out int vocabId))
                {
                    results.Add(vocabId);
                }
                else
                {
                    results.Add(vocab["[UNK]"]);
                }
            }

            return results.ToArray();
        }

        private List<string> WordPieceTokenize(string text)
        {
            List<string> outputTokens = [];
            // Assuming whitespace_tokenize is a method that splits the text into tokens.
            foreach (var token in WhitespaceTokenize(text))
            {
                var chars = token.ToCharArray();
                if (chars.Length > maxInputCharsPerWord)
                {
                    outputTokens.Add(unkToken);
                    continue;
                }

                bool isBad = false;
                int start = 0;
                List<string> subTokens = new List<string>();
                while (start < chars.Length)
                {
                    int end = chars.Length;
                    string curSubstr = null;
                    while (start < end)
                    {
                        string substr = new string(chars, start, end - start);
                        if (start > 0)
                        {
                            substr = "##" + substr;
                        }
                        if (vocab.ContainsKey(substr))
                        {
                            curSubstr = substr;
                            break;
                        }
                        end--;
                    }
                    if (curSubstr == null)
                    {
                        isBad = true;
                        break;
                    }
                    subTokens.Add(curSubstr);
                    start = end;
                }

                if (isBad)
                {
                    outputTokens.Add(unkToken);
                }
                else
                {
                    outputTokens.AddRange(subTokens);
                }
            }
            return outputTokens;
        }

        private IEnumerable<string> WhitespaceTokenize(string text)
        {
            // Implement or use an existing method to split the text on whitespace.
            // This is a placeholder for the actual implementation.
            return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
