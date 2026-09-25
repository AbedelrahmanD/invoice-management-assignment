namespace InvoiceManagement.Services.Implementations
{
    public class FlashMessageService
    {

        private readonly Dictionary<string, string> _flashStore = new();

        
        public void Set(string key, string message)
        {
            _flashStore.Add(key, message);
         }

        
        public bool Has(string key)
        {
            return _flashStore.ContainsKey(key) && !string.IsNullOrEmpty(_flashStore[key]);
        }
 
        public string? Get(string key)
        {
            if (_flashStore.TryGetValue(key, out var message))
            {
                _flashStore.Remove(key); 
                return message;
            }

            return null;
        }

    }
}
