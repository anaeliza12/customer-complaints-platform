<div align="center">
<h1>Case Canais</h1>

</div>

---

## 📑 About the project
Sistema desenvolvido em .NET para simular o recebimento e processamento de reclamações de clientes em diferentes canais para um cenário bancário.  

O sistema recebe dados de dois canais principais:

1. **Canal Físico:** documentos PDF via S3 (passivo)
2. **Canal Externo:** APIs simuladas (ativo)

---

## 🛠 Tech Stack

- .NET 10
- AWS: SQS, S3, EventBridge, Lambda, DynamoDB, SNS
- Node.js (APIs externas simuladas)  
- Arquitetura: Hexagonal
- Outras libs: DotNetEnv, JsonSerializer


## 🔎 Architecture & Domain Model

### Flow Diagram
```
Canal Físico (PDF no S3)       Canal Externo (APIs)
            │                          │
            ▼                          ▼
      EventBridge                EventBridge (Scheduler)
            │                          │
            ▼                          ▼
           SQS  ◄────────────── Lambda (coleta dados)
            │
            ▼
      Worker .NET
            │
            ▼
 Normalização e Enriquecimento
 (Textract fake + Datamesh fake)
            │
            ▼
      EventBridge
        │        │
        ▼        ▼
   Lambda → DynamoDB     SNS → Outros Sistemas
```

### Domain Model


-- criar domain model




---

## 💻 How to run the project

### Clone o projeto

```bash
git clone https://github.com/seuusuario/customer-complaints-platform.git

```

### Rode a Worker

```bash
# Run the project

dotnet run --project ApiChamados.Infrastructure/Worker

```


### Rode as mock API's

```bash
cd mock-apis/api-externa
node {api}.js
```

<br>

## ⚙️ Configuração variáveis de ambiente

```bash
AWS_ACCESS_KEY_ID=XXXX
AWS_SECRET_ACCESS_KEY=XXXX
AWS_REGION=XXXX

##complaints-queue-doc
SQS_QUEUE_NAME_DOC=XXXX
SQS_QUEUE_URL_DOC=XXXX


##complaints-queue-channel
SQS_QUEUE_NAME_CHANNEL=XXXX
SQS_QUEUE_URL_CHANNEL=XXXX

```


## 📂 Project File Tree

```
ApiChamados.sln
│
├─ src/
│   ├─ ApiChamados.Application/             # Casos de uso e orquestração
│   │   ├─ UseCases/
│   │   │   ├─ ReceberReclamacaoUseCase.cs  # Processa PDF ou JSON
│   │   │   ├─ GerarChamadoUseCase.cs
│   │   │   └─ SLAAlertUseCase.cs
│   │   └─ Services/                         # Serviços de orquestração se necessário
│   │
│   ├─ ApiChamados.Domain/                  # Núcleo de negócio
│   │   ├─ Reclamacao/
│   │   │   ├─ Reclamacao.cs
│   │   │   ├─ ReclamacaoId.cs
│   │   │   └─ Historico.cs
│   │   ├─ Chamado/
│   │   │   ├─ Chamado.cs
│   │   │   ├─ ChamadoId.cs
│   │   │   └─ SLA.cs
│   │   └─ Events/
│   │       ├─ ReclamacaoRecebida.cs
│   │       └─ ChamadoCriado.cs
│   │
│   ├─ ApiChamados.Ports/                   # Interfaces externas (Ports)
│   │   ├─ Repositories/
│   │   │   ├─ IReclamacaoRepository.cs
│   │   │   └─ IChamadoRepository.cs
│   │   ├─ Messaging/
│   │   │   ├─ IEventPublisher.cs           # Para publicar eventos
│   │   │   └─ ISqsListener.cs              # Interface do listener de SQS
│   │   └─ Storage/
│   │       └─ IPdfStorage.cs               # Upload/download de PDFs
│   │
│   ├─ ApiChamados.Adapters/                # Implementações concretas
│   │   ├─ Repositories/
│   │   │   ├─ PostgresReclamacaoRepository.cs
│   │   │   └─ PostgresChamadoRepository.cs
│   │   ├─ Messaging/
│   │   │   ├─ SqsPublisher.cs              # Publica eventos/JSON na fila
│   │   │   ├─ SnsPublisher.cs              # Notificações (se necessário)
│   │   │   └─ SqsListenerWorker.cs         # Worker que escuta SQS e processa mensagens
│   │   └─ Storage/
│   │       └─ S3PdfStorage.cs
│   │
│   ├─ ApiChamados.Infrastructure/         # Infraestrutura/AWS/eventos
│   │   ├─ Worker/                          # Serviços que rodam em background
│   │   │   └─ ReclamacaoSqsWorker.cs       # Escuta SQS, processa PDF/JSON e chama UseCase
│   │   └─ AwsConfig/                        # Configuração AWS
│   │       ├─ SqsConfig.cs
│   │       └─ S3Config.cs
│   │
│   └─ ApiChamados.Shared/                  # Utilities, logging, errors
│       ├─ Utils/
│       │   └─ DateTimeHelper.cs
│       ├─ Logging/
│       │   └─ Logger.cs
│       └─ Exceptions/
│           ├─ DomainException.cs
│           └─ ApplicationException.cs
│
└─ tests/
    ├─ ApiChamados.UnitTests/
    │   ├─ Domain/
    │   │   └─ ReclamacaoTests.cs
    │   ├─ Application/
    │   │   └─ ReceberReclamacaoUseCaseTests.cs
    │   └─ Adapters/
    │       └─ PostgresReclamacaoRepositoryTests.cs
    │
    └─ ApiChamados.IntegrationTests/
        └─ Adapters/
            ├─ S3PdfStorageIntegrationTests.cs
            └─ SqsPublisherIntegrationTests.cs

```


## 🔔 Monitoring & Observability

- Logs: registros de execução e erro no Worker .NET, Lambdas e consumo das filas SQS
- Métricas: contagem de mensagens processadas, falhas e tempo de execução
- Rastreabilidade: cada etapa do fluxo gera logs permitindo acompanhar o ciclo completo da reclamação
- Dead Letter Queue (DLQ): mensagens que falharem repetidamente são redirecionadas para análise posterior

`
O uso de SNS para alertas automáticos é considerado como evolução da solução, podendo ser integrado via CloudWatch Alarms em um cenário produtivo.
`

### 📌 Additional Information

APIs externas e Datamesh são simuladas para fins de teste do case.
Para rodar, crie suas próprias credenciais AWS em .env.

Exemplo de documentos PROCON e BACEN para S3 canal físico:
```
 /docs/BACEN - Registro de Reclamação.md
 /docs/PROCON - Reclamação do Consumidor.md
```

### 📄 Documentação

A documentação completa de arquitetura, fluxo, observabilidade e decisões técnicas está disponível em:

```
 /docs/architecture.md
```
