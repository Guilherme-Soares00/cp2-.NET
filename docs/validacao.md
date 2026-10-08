# Validação da CP2

Verificação executada em 8 de outubro de 2026.

## Ambiente

- Windows, SDK .NET 10.0.401 e runtime 10.0.12.
- EF Core e ferramenta `dotnet-ef` 10.0.12.
- Provider `MySql.EntityFrameworkCore` 10.0.9.
- MySQL 8.4.11 no container Docker `TDSPB`.

## Resultados

| Verificação | Resultado |
| --- | --- |
| Build da solução em Release | Zero erros e zero avisos. |
| Migration `InitialCreate` no MySQL | Aplicada; oito tabelas de domínio e histórico do EF Core. |
| Modelo versus migration | Sem mudanças pendentes. |
| Testes de integração | Dez execuções aprovadas em MySQL real. |
| Verificação HTTP | Dezessete requisições aprovadas. |
| Compatibilidade Windows PowerShell 5.1 | POST com UTF-8 retornou 201; PUT e GET preservaram os acentos. |

Os testes exercitam o catálogo completo, TPH, N:N, configuração de usuário, propriedades opcionais, unicidade de e-mail/configuração/número de episódio, integridade de FKs, restrição de notas, exclusões em cascata e CRUD do repositório com objetos rastreados e sem tracking. Também verificam o cancelamento de consultas.

A verificação HTTP cobre health, OpenAPI, CRUD de gêneros, `Location` na criação, descrição opcional, dados lidos depois da atualização, respostas 400 para entradas inválidas, 409 para duplicidade e 404 para IDs inexistentes.

Os dados HTTP criados durante a verificação foram removidos, e o schema temporário dos testes de integração foi excluído. O banco da aplicação permanece com o esquema disponível para utilização.
