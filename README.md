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

## Tecnologias

- ASP.NET Core 8
- FluentValidation
- QuickFIX/n (biblioteca FIX)
- Razor Pages
  
## Como Rodar

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 (ou VS Code com extensão C#)

### Executando pelo Visual Studio

1. Abra a solution `OrderGenerator.sln`
2. Defina `OrderGenerator.Web` como projeto de inicialização

A aplicação estará disponível em:
- https://localhost:5001
- http://localhost:5000

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

## Como Rodar

### Pré-requisitos

- .NET 8 SDK
- Visual Studio 2022 (ou VS Code com extensão C#)

### Executando pelo Visual Studio

1. Abra a solution `OrderAccumulator.sln`
2. Defina `OrderAccumulator.Fix` como projeto de inicialização

O servidor FIX estará aguardando conexões na porta configurada.

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

## Como Rodar

### Pelo Visual Studio

1. Abra o **Test Explorer** (menu Test > Test Explorer)
2. Clique em **Run All** ou pressione **Ctrl+R, A**

## Tecnologias
- xUnit (framework de testes)
- FluentAssertions (assertions legíveis)
- Moq (mocking de dependências)
