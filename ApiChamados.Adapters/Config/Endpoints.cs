namespace ApiChamados.Adapters.Config
{
    public class Endpoints
    {
        public DataMesh DataMesh { get; set; } = new DataMesh();
    }

    public class DataMesh
    {
        public string BaseUrl { get; set; }
        public string ClientesPath { get; set; }
    }
}
