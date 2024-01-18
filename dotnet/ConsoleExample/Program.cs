var dicPath = @"D:\DATASET\unidic-mecab-2.1.2_bin";
var vocabPath = @"D:\DATASET\bert-japanese\bert-base-japanese-v2\vocab.txt";
var tokenizer = new BertJapaneseTokenizer.BertJapaneseTokenizer(dicPath, vocabPath);

var sentence = "打ち合わせが終わった後にご飯を食べましょう.";
//var sentence = "ご飯を食べましょう.";
//var sentence = "打ち合わせ";

int[] tokenIds = tokenizer.EncodePlus(sentence);

Console.WriteLine($"Sentence: {sentence}");
Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");
