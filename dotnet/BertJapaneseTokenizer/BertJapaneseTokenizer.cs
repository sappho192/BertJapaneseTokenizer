﻿using MeCab;
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

        public (int[], int[]) EncodePlus(string sentence, bool addSpecialTokens = true)
        {
            int[] inputIds = [];

            sentence = doNormalize(sentence);
            string[] tokens = doWordTokenize(sentence);
            string[] splitTokens = doSubwordTokenize(tokens);
            inputIds = doConvertTokensToIds(splitTokens);
            inputIds = appendSpecialTokens(inputIds);

            int[] attentionMask = new int[inputIds.Length];
            for (int i = 0; i < attentionMask.Length; i++)
            {
                attentionMask[i] = 1;
            }

            return (inputIds, attentionMask);
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
                List<string> subTokens = [];
                while (start < chars.Length)
                {
                    int end = chars.Length;
                    string curSubstr = null;
                    while (start < end)
                    {
                        string substr = new(chars, start, end - start);
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

        private static IEnumerable<string> WhitespaceTokenize(string text)
        {
            // Implement or use an existing method to split the text on whitespace.
            // This is a placeholder for the actual implementation.
            return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string Decode(int[] ids, bool skipSpecialTokens = true)
        {
            StringBuilder sb = new();
            foreach (var id in ids)
            {
                if (skipSpecialTokens && (id < 14)) // 14 == <unused9>
                {
                    continue;
                }
                var token = vocab.FirstOrDefault(x => x.Value == id).Key;
                if (token != null)
                {
                    // Remove ## from the beginning of the token
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
