using BertJapaneseTokenizer;

var dicPath = @"D:\DATASET\unidic-mecab-2.1.2_bin";
//var vocabPath = @"D:\DATASET\bert-japanese\bert-base-japanese-v2\vocab.txt";
var vocabPath = await HuggingFace.GetVocabFromHub("cl-tohoku/bert-base-japanese-v2");
var tokenizer = new BertJapaneseTokenizer.BertJapaneseTokenizer(dicPath, vocabPath);

var sentence = "打ち合わせが終わった後にご飯を食べましょう。";
//var sentence = "ご飯を食べましょう。";
//var sentence = "打ち合わせ";

(var tokenIds, var attentionMask) = tokenizer.EncodePlus(sentence);

Console.WriteLine($"Sentence: {sentence}");
Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");
