# Automatizador Cards Anki API <br>

<p align="center">
	<img src="https://img.shields.io/badge/Framework-dotnet-blue"/> 
	<img src="https://img.shields.io/badge/Framework%20version-dotnet%208-blue"/> 
	<img src="https://img.shields.io/badge/Language-C%23-blue"/> 
	<img src="https://img.shields.io/badge/Status-development-green"/>
</p>

Esta é uma Web API feita em .NET 8 para automatizar a criação de cards no meu Anki que auxilia nos meus estudos de inglês. <br/><br/>


## O que é Anki? 📝 <br/>

O Anki é um software que auxilia os estudos através de cards que você cria e os revisa periodicamente para memorizar algum conteúdo. <br/><br/>


## Como utilizo o Anki para estudar inglês? 📝 <br/>

Para melhorar o vocabulário em inglês utilizado o Anki, uma das formas de estudar é pegar uma palavra que não se sabe o significado, 
aplicar ela numa frase sendo essa palavra a única que não se sabe o significado na frase, colocar na parte da frente do card 
no Anki e na parte de trás o significado, dessa forma é mais fácil memorizar o significado. <br/><br/>


## O que o automatizador-cards-anki ajuda nesse processo? 📝 <br/>

Utilizando essa API, o processo para a criação dos cards para o estudo em inglês fica muito mais prático. É possível mandar um 
conjunto de palavras em inglês para aprender o significado e a API busca no ChatGPT uma frase com essa palavra e também o significado.
Depois é chamada a API que o Anki disponibiliza para criar os cards com as respostas do ChatGPT. Dessa forma então não é necessário
criar os cards um por um manualmente. <br/><br/>


## Documentação da API 📝 <br/>

O endpoint da API é documentada usando o Swagger.

Abra o navegador em http://localhost:5115/swagger/index.html. Isto ixibirá a interface de usuário Swagger, que fornece uma interface
amigável para explorar os endpoints da API. Ela fornece o seguinte endpoint:

- `POST /Anki/insert-cards` - adiciona novos cards no Anki.<br/><br/>


## Instalação :wrench: <br/>

1. Instale .NET 8 se você não tem. Você pode baixar [here](https://dotnet.microsoft.com/pt-br/download/dotnet/8.0).
2. Clone o  repositório para sua máquina local.`https://github.com/MarcioOlv95/automatizador-cards-anki.git`
3. Próximo, navegue até o diretório do projeto e execute o seguinte comando para restaurar as dependências:
`dotnet restore`
4. Finalmente, execute o seguinte comando para iniciar a API:
`dotnet run`
5. O aplicativo começará a ouvir em http://localhost:5115 <br/><br/>


## Próximas implementações :dart: <br/>

- Inserir em cada card no Anki uma imagem fornecida pelo ChatGPT que ajude a lembrar o significado da palavra em inglês. <br/><br/>


 ## Libraries and Backages 🛠️
- [Moq](https://www.nuget.org/packages/Moq)
- [MediatR](https://www.nuget.org/packages/MediatR)
- [FluentValidation](https://www.nuget.org/packages/fluentvalidation/)
- [OpenAI](https://www.nuget.org/packages/OpenAI)
- [AutoFixture](https://www.nuget.org/packages/AutoFixture)
<br/><br/>
