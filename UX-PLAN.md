# Usabilidade da Fábrica Maciel

## Objetivo e resultado
Revisão concluída em 20/09/2026 para o trabalho diário da fábrica de panelas: administrar usuários, cadastrar materiais e produtos, movimentar cadastros existentes no estoque e registrar pedidos de clientes.

## Requisitos e evidências
| Necessidade | Implementação | Validação |
| --- | --- | --- |
| Navegação simples, sem links redundantes | Marca sem link duplicado; Visão geral, Pedidos, Estoque e cadastros administrativos; Privacy removida | Layout e HomeController revisados; navegação conferida no navegador |
| Administrador cria os acessos da equipe | Usuários com nome, e-mail e nível de acesso; cadastro público direcionado ao administrador | Criação e acesso de usuário comum testados; páginas administrativas protegidas; último administrador não pode perder seu nível |
| Materiais: acessórios e discos | Abas, buscas, peso por unidade, medidas e cálculo opcional do disco | Pomel de 25 g e disco de 0,1 kg cadastrados em banco isolado; abas e formulário conferidos no celular |
| Produtos: peças fabricadas | Nome/número, disco principal, tampa, acessórios e preços | Caneco 12 com um pomel e preço de R$ 20,50 salvo; edição conferida no navegador |
| Estoque apenas de cadastros existentes | Seletores do catálogo; kg para materiais, unidades para produtos; mensagens junto aos campos | ID inexistente rejeitado; entrada de materiais e produção de 2 unidades com saldos de 0,8 kg de disco e 0,95 kg de acessório; saída acima do saldo rejeitada |
| Pedidos por cliente e quantidade | Cliente criado na própria tela, preço automático e totais por linha/pedido | Cadastro de cliente via modal e pedido de 2 unidades a R$ 20,50 salvo pelo navegador por R$ 41,00; alteração de etapa testada |
| Experiência limpa e profissional | Identidade visual compartilhada, orientações de trabalho, busca, estados vazios, validação em português e valores brasileiros | Desktop e celular de 390 px conferidos; menu recolhível, troca de campos e rolagem de tabelas pelo teclado verificados |
| Feedback em erros | Exclusões de cadastros vinculados retornam aviso; página de erro em português | Exclusão de produto, acessório e disco em uso bloqueada pelos testes |

## Verificação executada
- `dotnet build Fabrica.csproj --no-restore --tl:off`: passou sem avisos nem erros na versão final.
- `tests/ux-smoke.ps1`: passou no banco novo e isolado `C:/Users/mauri/AppData/Local/Temp/Fabrica-UX-Validation/ux-audit-20260920.db`. O script pressupõe banco vazio, porta 5001 e IDs iniciais; não repetir no banco real ou em uma base já populada.
- Navegador: início desktop, materiais/discos, produto, usuários, estoque e pedidos em celular; cliente e pedido gravados apenas na base de teste. Busca sem resultados e edição de preço com vírgula verificadas durante a revisão.
- Navegador na porta 5000: login e visual final conferidos após recompilação e reinício. CSS e Bootstrap disponíveis.

## Regras preservadas
- Cadastrar material/produto não cria saldo; quantidades entram no Estoque.
- Entrada de produto consome componentes quando há saldo. Se faltar material, uma confirmação explícita permite entrada sem consumo.
- Salvar ou entregar pedido não baixa estoque automaticamente; a tela orienta registrar a saída no Estoque.
- Os testes de gravação usaram banco separado. Não reverter as alterações preexistentes de `Fabrica.db` nem incluir os arquivos de banco como mudanças de UX.

## Executar localmente
Na pasta do projeto: `dotnet run --launch-profile http --urls http://localhost:5000`.
O perfil `http` habilita o ambiente Development e os arquivos estáticos locais. Após mudanças visuais, recarregar o navegador com Ctrl+F5.
