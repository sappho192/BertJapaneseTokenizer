# BertJapaneseTokenizer
Minimal Tokenizer implementation of BertJapanese([cl-tohoku/bert-base-japanese](https://github.com/cl-tohoku/bert-japanese)) in C#

# Quickstart
1. Just add `BertJapaneseTokenizer` package from Nuget.
2. Download unidic mecab dictionary `unidic-mecab-2.1.2_bin.zip` from https://clrd.ninjal.ac.jp/unidic_archive/cwj/2.1.2/ and unzip the archive into somewhere.
3. Download vocab file BertJapanese from Huggingface. For example, `vocab.txt` of bert-base-japanese-v2 can be accessed from [[here](https://huggingface.co/tohoku-nlp/bert-base-japanese-v2/tree/main)].  
**(Or you can simply use my extension method `GetVocabFromHub()`. See the example below.)**
4. Check the example code below and you are good to go.

```CSharp
using BertJapaneseTokenizer;

var dicPath = @"D:\DATASET\unidic-mecab-2.1.2_bin";
//var vocabPath = @"D:\DATASET\bert-japanese\bert-base-japanese-v2\vocab.txt";
var vocabPath = await HuggingFace.GetVocabFromHub("tohoku-nlp/bert-base-japanese-v2");
var tokenizer = new BertJapaneseTokenizer.BertJapaneseTokenizer(dicPath, vocabPath);

var sentence = "打ち合わせが終わった後にご飯を食べましょう。";
//var sentence = "ご飯を食べましょう。";
//var sentence = "打ち合わせ";

(var tokenIds, var attentionMask) = tokenizer.EncodePlus(sentence);

Console.WriteLine($"Sentence: {sentence}");
Console.WriteLine($"Token IDs: {string.Join(", ", tokenIds)}");

var decoded = tokenizer.Decode(tokenIds);
Console.WriteLine($"Decoded: {decoded}");
```

# To-do List
- [x] Implement Decode() method
- [ ] Support BPE-type vocabulary (like `cl-tohoku/bert-base-japanese-char`)
