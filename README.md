<div align="center">
<h1>Case Canais</h1>

![logo_case_canais](https://via.placeholder.com/150?text=Case+Canais)  
</div>

---

## 📑 About the project
**Case Canais** é um sistema desenvolvido em .NET para simular o recebimento e processamento de reclamações de clientes em diferentes canais para um cenário bancário.  

O sistema recebe dados de dois canais principais:

1. **Canal Físico:** documentos PDF digitalizados via scanner enviados para o S3.  
2. **Canal Externo:** APIs externas simuladas que retornam reclamações (mocadas).  

Após receber os dados, o sistema:  

- Normaliza e categoriza as reclamações  
- Enriquecimento do histórico do cliente via Datamesh fake  
- Persistência no **DynamoDB**  
- Notificação via **SNS**  
- Processamento assíncrono via **SQS** e **EventBridge**  

O objetivo é demonstrar **arquitetura hexagonal**, uso de AWS e boas práticas em **processamento assíncrono**.

---

## 🔎 Architecture & Domain Model

### Flow Diagram
> (Insira aqui o diagrama de fluxo do sistema, exemplo: `S3 → Worker → Textract → Datamesh → EventBridge → Lambda → DynamoDB + SNS`)

![Architecture Diagram](./assets/architecture_diagram.png)

### Domain Model


-- criar domain model




---

## 💻 How to run the project

### Clone the project

```bash
git clone https://github.com/seuusuario/case-canais.git
cd case-canais


```

### Rode a Worker

```bash
# Run the project

dotnet run --project ApiChamados.Infrastructure/Worker

```


### Rode as mock API's

```bash
cd mock-apis/api-externa
node index.js
```

<br>

### Configuração variáveis de ambiente

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


### 📂 Project File Tree

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
<img width="766" height="1635" alt="image" src="https://github.com/user-attachments/assets/4900682c-dbea-4a92-87ae-c1296e28a260" />



```


### 🔔 Monitoring & Alerts

- CloudWatch Metrics / Logs: monitor Worker, Lambdas e filas SQS
- SNS: alertas em caso de erro no processamento
- Dead Letter Queue (DLQ): mensagens que falharam várias vezes
- Evitar gargalos:
  - Filas separadas por canal
  - Escalabilidade de Workers conforme backlog
  - Processamento assíncrono e desacoplado via EventBridge
 
🛠 Tech Stack

- .NET 7
- AWS: SQS, S3, EventBridge, Lambda, DynamoDB, SNS
- Node.js para APIs mocadas
- Arquitetura: Hexagonal
- Outras libs: DotNetEnv, JsonSerializer



### 📌 Additional Information

APIs externas e Datamesh são simuladas para fins de teste do case.
Para rodar, crie suas próprias credenciais AWS em .env.

O fluxo completo inclui PDF → Textract → normalização → categorização → EventBridge → Lambda → DynamoDB + SNS.> Estrutura principal de entidades e objetos do sistema:

