﻿<h1 align="center"> Hackaton Pós Tech - Health&Med </h1>

![Capa com o nome do curso da pós graduação](./Assets/capa-readme.jpg)

![Badge em Desenvolvimento](http://img.shields.io/static/v1?label=STATUS&message=FINALIZADO&color=RED&style=for-the-badge)
<br>![GitHub Org's stars](https://img.shields.io/github/stars/DalianeLeme?style=social)</br>

# Índice 

* [Descrição do Projeto](#Descrição-do-projeto)
* [Requisitos não funcionais](#Requisitos-não-funcionais)
* [Diagrama requisitos funcionais](#Diagrama-requisitos-funcionais)
* [Técnicas e tecnologias utilizadas](#Técnicas-e-tecnologias-utilizadas)

# :pushpin: Descrição do projeto
API em .NET8 feita para entrega do Hackaton Pós Tech FIAP. <br>
Sistema para startup Health&Med, proporcionando a possibilidadce de agendamendo e realização . </br>

<br></br>

# Requisitos não funcionais

- `Alta disponibilidade`: </br>
:bookmark_tabs:Kubernetes com réplicas e volumes (ConfigMap e deployment). </br>
:bookmark_tabs:Arquitetura por eventos evitando pontos únicos de falaha. </br>

- `Escalabilidade`:  </br>
:small_red_triangle_down: Microsserviços independentes que podem ser escalados individualmente conforme carga. </br>
:small_red_triangle_down: Aumenta ou reduz o número de pods confome uso de CPU/Memória. </br>

- `Segurança`:  </br>
:small_blue_diamond: Autenticação com JWT, garantindo acesso seguro e com controle por perfil (médico/paciente). </br>
:small_blue_diamond: Criptografia de dados sensíveis. </br>

# Diagrama requisitos funcionais


# :heavy_check_mark: Técnicas e tecnologias utilizadas
`.NET8` `C#` `SQL Server` `GitHub Actions`  `Testes unitários` `xUnit` `EntityFramework`
`FluentValidator` `RabbitMQ` `Eventos` `Kubernetes` `Docker`
<br></br>

# :busts_in_silhouette: Autores
[<img loading="lazy" src="https://avatars.githubusercontent.com/u/55365144?v=4" width=115><br><sub>Daliane Leme</sub>](https://github.com/DalianeLeme)
<br></br>
