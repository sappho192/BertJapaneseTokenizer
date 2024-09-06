namespace BertJapaneseTokenizer
{
    public static class HuggingFace
    {
        /// <summary>
        /// <para>Download the vocab.txt file in the hub.</para>
        /// <para>Example of supported hub: cl-tohoku/bert-base-japanese, cl-tohoku/bert-large-japanese-v2, etc.</para>
        /// <para>If you specify the destination directory, vocab.txt will be saved in like: destDir/bert-base-japanese-v2/vocab.txt</para>
        /// </summary>
        /// <param name="hubName"></param>
        /// <param name="destinationDirectory"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static async Task<string> GetVocabFromHub(string hubName,
            string destinationDirectory = "", bool overwrite = true)
        {
            // Example of hubName: cl-tohoku/bert-large-japanese
            // We need to separate the hubName, delimiter is '/'
            // And then concatenate the path using Path.Combine
            string[] sepPath = hubName.Split('/');
            string directory = Path.Combine(sepPath);
            string vocabPath;
            if (string.IsNullOrEmpty(destinationDirectory))
            {
                vocabPath = Path.Combine(directory, "vocab.txt");
            }
            else
            {
                directory = Path.Combine(destinationDirectory, sepPath[1]);
                vocabPath = Path.Combine(directory, "vocab.txt");
            }
            // Check if the vocab.txt file exists in the directory
            // Return path if it exists
            if (File.Exists(vocabPath) && !overwrite)
            {
                return vocabPath;
            }

            // Download the vocab.txt file from the hub
            // Example url: https://huggingface.co/cl-tohoku/bert-large-japanese/resolve/main/vocab.txt?download=true
            var url = $"https://huggingface.co/{hubName}/resolve/main/vocab.txt?download=true";
            // Using HttpClient, save into a path {hubName}/vocab.txt, and return the path
            var client = new HttpClient();
            var response = await client.GetAsync(url);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Cannot download vocab.txt from {url}");
            }
            // Create the directory if it doesn't exist
            Directory.CreateDirectory(directory);
            using (var fileStream = File.Create(vocabPath))
            {
                await response.Content.CopyToAsync(fileStream);
            }
            return vocabPath;
        }
    }
}
