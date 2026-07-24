    class NoteManager
    {
        private string filePath;

        public NoteManager(string filePath)
        {
            this.filePath = filePath;
        }

        public void WriteNote(string content)
        {
            File.WriteAllText(filePath, content); // simplest way — overwrites the file
            Console.WriteLine("Note saved.");
        }

        public void AppendNote(string content)
        {
            File.AppendAllText(filePath, content + Environment.NewLine); // adds without overwriting
            Console.WriteLine("Note appended.");
        }

        public string ReadNote()
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
            return "No note found.";
        }

        // Using a Stream directly — more control, used for large files
        public void WriteWithStream(string content)
        {
            using (StreamWriter writer = new StreamWriter(filePath, append: true))
            {
                writer.WriteLine(content);
            } // 'using' auto-closes the file when done, even if an error occurs
            Console.WriteLine("Written using StreamWriter.");
        }
    }