# OrderGenerator

Sistema de geração e processamento de ordens utilizando o protocolo FIX (Financial Information Exchange).

## Sobre o Projeto

OrderGenerator é uma aplicação ASP.NET Core 8 que permite enviar ordens de negociação através do protocolo FIX. O projeto foi desenvolvido seguindo os princípios de Clean Architecture, separando responsabilidades em camadas distintas.

## Arquitetura

O projeto está organizado em três camadas:

```
OrderGenerator/
├── OrderGenerator.Application/    # Lógica de negócio
├── OrderGenerator.Fix/            # Integração com protocolo FIX
└── OrderGenerator.Web/            # API e interface web
```

### OrderGenerator.Application

Camada de aplicação contendo regras de negócio:

| Pasta/Arquivo | Responsabilidade |
|---------------|------------------|
| `Dto/OrderDto.cs` | Objeto de transferência com dados da ordem (Symbol, Side, Quantity, Price) |
| `Dto/OrderResult.cs` | Resultado do processamento (Accepted, Message) |
| `Interfaces/IFixOrderSender.cs` | Contrato para envio de ordens via FIX |
| `Interfaces/ISendOrder.cs` | Contrato do serviço de envio |
| `Service/SendOrder.cs` | Implementação da lógica de envio de ordens |
| `Validator/OrderDtoValidator.cs` | Validações usando FluentValidation |

### OrderGenerator.Fix

Camada de infraestrutura para comunicação FIX:

| Arquivo | Responsabilidade |
|---------|------------------|
| `FixApplication.cs` | Implementação da aplicação FIX (callbacks de sessão) |
| `FixOrderSender.cs` | Construção e envio de mensagens FIX |
| `FixStart.cs` | Serviço hospedado que inicializa a conexão FIX |

### OrderGenerator.Web

Camada de apresentação MVC:

| Pasta/Arquivo | Responsabilidade |
|---------------|------------------|
| `Controllers/HomeController.cs` | Endpoints da API |
| `Views/Home/Index.cshtml` | Interface web |
| `Models/ErrorViewModel.cs` | Modelo para exibição de erros |
| `appsettings.json` | Configurações da aplicação |
| `FIX44.xml` | Dicionário de dados FIX 4.4 |
| `initiator.cfg` | Configuração do cliente FIX |
| `Program.cs` | Ponto de entrada e configuração de DI |

## Fluxo de Processamento

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   Cliente   │────▶│ Controller  │────▶│ SendOrder   │────▶│FixOrderSender│
│  (Request)  │     │  (Web)      │     │(Application)│     │   (Fix)     │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                               │                    │
                                               ▼                    ▼
                                        ┌─────────────┐     ┌─────────────┐
                                        │  Validator  │     │ Servidor FIX│
                                        └─────────────┘     └─────────────┘
```

1. Cliente envia requisição HTTP com dados da ordem
2. Controller recebe e repassa para o serviço
3. SendOrder valida os dados usando FluentValidation
4. Se válido, FixOrderSender constrói a mensagem FIX
5. Mensagem é enviada ao servidor FIX configurado

## Como Rodar

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 (ou VS Code com extensão C#)

### Executando pelo Visual Studio

1. Abra a solution `OrderGenerator.sln`
2. Defina `OrderGenerator.Web` como projeto de inicialização
3. Pressione **F5** para executar em modo debug (ou Ctrl+F5 sem debug)

### Executando pelo Terminal

```bash
# Na pasta raiz da solution
cd OrderGenerator.Web
dotnet run
```

A aplicação estará disponível em:
- https://localhost:5001
- http://localhost:5000

## Configuração FIX
O arquivo `initiator.cfg` contém as configurações de conexão com o servidor FIX:

## Tecnologias

- ASP.NET Core 8
- FluentValidation
- QuickFIX/n (biblioteca FIX)
- Razor Pages

## Estrutura de Pastas Geradas em Runtime
```
OrderGenerator.Web/
├── log/       # Logs de comunicação FIX
└── store/     # Persistência de estado das sessões FIX
```
Estas pastas são criadas automaticamente quando a aplicação inicia.

# OrderAccumulator

Sistema de acumulação de ordens e cálculo de exposição financeira utilizando o protocolo FIX (Financial Information Exchange).

## Sobre o Projeto

OrderAccumulator é uma aplicação .NET 8 que atua como servidor FIX, recebendo ordens de negociação e calculando a exposição financeira por símbolo. O projeto segue os princípios de Clean Architecture e Domain-Driven Design (DDD).

## Arquitetura

O projeto está organizado em três camadas:

```
OrderAccumulator/
├── OrderAccumulator.Application/    # Lógica de aplicação
├── OrderAccumulator.Domain/         # Entidades e regras de domínio
└── OrderAccumulator.Fix/            # Servidor FIX (Acceptor)
```

### OrderAccumulator.Domain

Camada de domínio contendo entidades e regras de negócio:

| Pasta/Arquivo | Responsabilidade |
|---------------|------------------|
| `Entities/Order.cs` | Entidade que representa uma ordem de negociação |
| `Entities/SymbolExposure.cs` | Entidade que representa a exposição por símbolo |
| `Interface/ExposureCalculator.cs` | Implementação do cálculo de exposição |
| `Interface/IExposureCalculator.cs` | Contrato para cálculo de exposição |
| `ValueObject/Money.cs` | Value Object para valores monetários |
| `ValueObject/OrderSideType.cs` | Enum para lado da ordem (Buy/Sell) |
| `ValueObject/Symbol.cs` | Value Object para símbolo do ativo |

### OrderAccumulator.Application

Camada de aplicação que orquestra o fluxo:

| Pasta/Arquivo | Responsabilidade |
|---------------|------------------|
| `Interface/IOrderProcessor.cs` | Contrato para processamento de ordens |
| `Service/OrderProcessor.cs` | Implementação do processador de ordens |

### OrderAccumulator.Fix

Camada de infraestrutura que implementa o servidor FIX:

| Arquivo | Responsabilidade |
|---------|------------------|
| `FixApplication.cs` | Implementação do acceptor FIX (recebe conexões e ordens) |
| `acceptor.cfg` | Configuração do servidor FIX |
| `FIX44.xml` | Dicionário de dados FIX 4.4 |
| `Program.cs` | Ponto de entrada da aplicação |

## Fluxo de Processamento

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ Cliente FIX │────▶│    Fix      │────▶│  Order      │────▶│  Exposure   │
│  (Ordem)    │     │ Application │     │  Processor  │     │ Calculator  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                                   │
                                                                   ▼
                                                            ┌─────────────┐
                                                            │   Symbol    │
                                                            │  Exposure   │
                                                            └─────────────┘
```

1. Cliente FIX conecta e envia ordem (NewOrderSingle)
2. FixApplication recebe a mensagem e extrai os dados
3. OrderProcessor processa a ordem recebida
4. ExposureCalculator atualiza a exposição do símbolo
5. SymbolExposure armazena a posição acumulada

## Cálculo de Exposição

A exposição é calculada considerando:

- **Compra (Buy):** Adiciona à exposição (+)
- **Venda (Sell):** Subtrai da exposição (-)

```
Exposição = Σ (Quantidade × Preço × Lado)

Onde Lado = +1 para Buy, -1 para Sell
```

**Exemplo:**
| Ordem | Símbolo | Lado | Qtd | Preço | Exposição |
|-------|---------|------|-----|-------|-----------|
| 1 | PETR4 | Buy | 100 | 35.00 | +3.500,00 |
| 2 | PETR4 | Sell | 50 | 36.00 | +1.700,00 |
| 3 | PETR4 | Buy | 200 | 34.50 | +8.600,00 |

## Como Rodar

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 (ou VS Code com extensão C#)

### Executando pelo Visual Studio

1. Abra a solution `OrderAccumulator.sln`
2. Defina `OrderAccumulator.Fix` como projeto de inicialização
3. Pressione **F5** para executar em modo debug

### Executando pelo Terminal

```bash
cd OrderAccumulator.Fix
dotnet run
```

O servidor FIX estará aguardando conexões na porta configurada.

## Configuração FIX
O arquivo `acceptor.cfg` contém as configurações do servidor FIX

## Tecnologias
- .NET 8
- QuickFIX/n (biblioteca FIX)
- Domain-Driven Design (DDD)

# OrderTests

Projeto de testes unitários para as soluções OrderAccumulator e OrderGenerator.

## Estrutura

```
tests/
└── OrderTests/
    ├── OrderAccumulatorTests/    # Testes do OrderAccumulator
    └── OrderGeneratorTests/      # Testes do OrderGenerator
```

## Testes do OrderAccumulator

| Arquivo | O que testa |
|---------|-------------|
| `Objects/MoneyTest.cs` | Value Object Money (operações monetárias) |
| `Objects/OrderTest.cs` | Entidade Order (criação e propriedades) |
| `Objects/SymbolTest.cs` | Value Object Symbol (validações) |
| `ExposureCalculatorTest.cs` | Cálculo de exposição financeira |
| `OrderProcessorTest.cs` | Processamento e acumulação de ordens |

## Testes do OrderGenerator

| Arquivo | O que testa |
|---------|-------------|
| `HomeControllerTest.cs` | Endpoints da API (requests/responses) |
| `OrderDtoValidatorTest.cs` | Validações do DTO com FluentValidation |
| `SendOrderTest.cs` | Serviço de envio de ordens |

## Como Rodar

### Pelo Visual Studio

1. Abra o **Test Explorer** (menu Test > Test Explorer)
2. Clique em **Run All** ou pressione **Ctrl+R, A**

### Pelo Terminal

```bash
# Rodar todos os testes
dotnet test

# Rodar com detalhes
dotnet test --verbosity normal

# Rodar testes específicos
dotnet test --filter "FullyQualifiedName~OrderAccumulatorTests"
dotnet test --filter "FullyQualifiedName~OrderGeneratorTests"
```

## Tecnologias
- xUnit (framework de testes)
- FluentAssertions (assertions legíveis)
- Moq (mocking de dependências)
