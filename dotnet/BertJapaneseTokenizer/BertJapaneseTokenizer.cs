using MeCab;
using System.Text;

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
        public Dictionary<int, string>? ReverseVocab
        {
            get
            {
                return reverseVocab;
            }
        }
        public MeCabTagger? Tagger
        {
            get
            {
                return tagger;
            }
        }
        public const string TOKEN_PAD = "[PAD]";
        public const string TOKEN_UNK = "[UNK]";
        public const string TOKEN_CLS = "[CLS]";
        public const string TOKEN_SEP = "[SEP]";
        public const string TOKEN_MASK = "[MASK]";

        private readonly Dictionary<string, int> vocab = new Dictionary<string, int>();
        private readonly Dictionary<int, string> reverseVocab;
        private readonly MeCabTagger? tagger;
        private const int maxInputCharsPerWord = 100;
        private static readonly char[] separator = new[] { ' ', '\t', '\n', '\r' };

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
            // Create a reverse lookup dictionary
            reverseVocab = vocab.ToDictionary(pair => pair.Value, pair => pair.Key);

            var parameter = new MeCabParam(dictPath);
            tagger = MeCabTagger.Create(parameter);
        }

        public (int[], int[]) EncodePlus(string sentence, bool addSpecialTokens = true)
        {
            int[] inputIds;

            sentence = doNormalize(sentence);
            string[] tokens = doWordTokenize(sentence);
            string[] splitTokens = doSubwordTokenize(tokens);
            inputIds = doConvertTokensToIds(splitTokens);
            inputIds = appendDefaultSpecialTokens(inputIds);

            int[] attentionMask = Enumerable.Repeat(1, inputIds.Length).ToArray();

            return (inputIds, attentionMask);
        }

        private int[] appendDefaultSpecialTokens(int[] result)
        {
            // Initialize the list with the expected capacity to avoid resizing
            List<int> results = new List<int>(result.Length + 2);
            results.Add(vocab[TOKEN_CLS]);
            results.AddRange(result);
            results.Add(vocab[TOKEN_SEP]);
            return results.ToArray();
        }

        private string doNormalize(string sentence)
        {
            return sentence.Normalize(System.Text.NormalizationForm.FormKC);
        }

        private string[] doWordTokenize(string sentence)
        {
            List<string> result = new List<string>();
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
            List<string> result = new List<string>();

            foreach (var token in tokens)
            {
                var subTokens = wordPieceTokenize(token);
                result.AddRange(subTokens);
            }

            return result.ToArray();
        }

        private int[] doConvertTokensToIds(string[] splitTokens)
        {
            List<int> results = new List<int>();

            foreach (var token in splitTokens)
            {
                if (vocab.TryGetValue(token, out int vocabId))
                {
                    results.Add(vocabId);
                }
                else
                {
                    results.Add(vocab[TOKEN_UNK]);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// <para>Performs wordpiece tokenization. 
        /// Original code is from Transformers.models.bert_japanese.tokenization_bert_japanese.py
        /// </para>
        /// <para>
        /// See https://github.com/huggingface/transformers/blob/a7cab3c283312b8d4de5df3bbe719971e24f4281/src/transformers/models/bert_japanese/tokenization_bert_japanese.py#L902-L948
        /// </para>
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<string> wordPieceTokenize(string text)
        {
            List<string> outputTokens = new List<string>();
            foreach (var token in whitespaceTokenize(text))
            {
                if (token.Length > maxInputCharsPerWord)
                {
                    outputTokens.Add(TOKEN_UNK);
                    continue;
                }

                int start = 0;
                while (start < token.Length)
                {
                    int end = token.Length;
                    string curSubstr = null;
                    while (start < end)
                    {
                        string substr = (start == 0) ? token[start..end] : "##" + token[start..end];
                        if (vocab.ContainsKey(substr))
                        {
                            curSubstr = substr;
                            break;
                        }
                        end--;
                    }
                    if (curSubstr == null)
                    {
                        outputTokens.Add(TOKEN_UNK);
                        break;
                    }
                    outputTokens.Add(curSubstr);
                    start = end;
                }
            }
            return outputTokens;
        }


        private static IEnumerable<string> whitespaceTokenize(string text)
        {
            // Implement or use an existing method to split the text on whitespace.
            // This is a placeholder for the actual implementation.
            return text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        public string Decode(int[] ids, bool skipSpecialTokens = true)
        {
            StringBuilder sb = new();
            foreach (var id in ids)
            {
                if (skipSpecialTokens)
                {
                    if (id == vocab[TOKEN_CLS] || id == vocab[TOKEN_SEP] ||
                    id == vocab[TOKEN_PAD] || id == vocab[TOKEN_MASK] ||
                    id == vocab[TOKEN_UNK])
                    {
                        continue;
                    }
                }
                if (reverseVocab.TryGetValue(id, out string token))
                {
                    if (token.StartsWith("##"))
                    {
                        token = token.Substring(2);
                    }
                    sb.Append(token);
                }
            }
            return sb.ToString();
        }
    }
}
