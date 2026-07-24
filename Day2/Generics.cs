// GenericBox.cs
 
    class GenericBox<T>   // T is a placeholder type, decided when you use the class
    {
        private T? item;

        public void Store(T newItem)
        {
            item = newItem;
        }

        public T Retrieve()
        {
        return item;
        }
}
