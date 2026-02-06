using ApiChamados.Adapters.Config;
using ApiChamados.Application.Ports.Data;
using ApiChamados.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ApiChamados.Adapters.Data.DataMesh
{
    public class DataMeshAdapter : IDataMeshService
    {
        private readonly HttpClient _httpClient;
        private readonly Endpoints _endpoints;
        private readonly ILogger<DataMeshAdapter> _logger;

        public DataMeshAdapter(
            HttpClient httpClient,
            IOptions<Endpoints> options,
            ILogger<DataMeshAdapter> logger)
        {
            _httpClient = httpClient;
            _endpoints = options.Value;
            _logger = logger;
        }

        public async Task<PerfilCliente> ObterPerfilClienteAsync(string cpf)
        {
            var sw = Stopwatch.StartNew();
            var path = $"{_endpoints.DataMesh.ClientesPath}/{cpf}";

            _logger.LogInformation("Metrica:InicioChamadaDataMesh | CPF: {CPF} | URL: {FullUrl}",
                cpf, $"{_httpClient.BaseAddress}{path}");

            try
            {
                var response = await _httpClient.GetAsync(path);
                sw.Stop();

                _logger.LogInformation("Metrica:FimChamadaDataMesh | TempoMS: {Duracao} | StatusCode: {StatusCode} | CPF: {CPF}",
                    sw.ElapsedMilliseconds, (int)response.StatusCode, cpf);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Metrica:ClienteNaoEncontradoDataMesh | CPF: {CPF}", cpf);
                    throw new Exception($"Cliente {cpf} não encontrado no DataMesh.");
                }

                response.EnsureSuccessStatusCode();

                var perfil = await response.Content.ReadFromJsonAsync<PerfilCliente>();

                if (perfil == null)
                {
                    _logger.LogError("Metrica:ErroDeserializacaoDataMesh | CPF: {CPF}", cpf);
                    throw new Exception("Erro ao deserializar perfil do cliente.");
                }

                return perfil;
            }
            catch (HttpRequestException ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Metrica:FalhaRedeDataMesh | Erro: {Msg} | TempoMS: {Duracao} | CPF: {CPF}",
                    ex.Message, sw.ElapsedMilliseconds, cpf);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Metrica:ErroInesperadoDataMesh | Erro: {Msg} | CPF: {CPF}", ex.Message, cpf);
                throw;
            }
        }
    }
}