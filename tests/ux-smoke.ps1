# Run only against the separate UX validation instance, with a fresh test database.
$ErrorActionPreference = 'Stop'
$baseUrl = 'http://localhost:5001'
$session = [Microsoft.PowerShell.Commands.WebRequestSession]::new()
function Page($path) { Invoke-WebRequest ($baseUrl + $path) -WebSession $session }
function Submit($pagePath, $action, $values) {
    $page = Page $pagePath
    $token = [regex]::Match($page.Content, 'name="__RequestVerificationToken" type="hidden" value="([^"]+)"').Groups[1].Value
    if (!$token) { throw "Missing antiforgery token: $pagePath" }
    $values.__RequestVerificationToken = $token
    Invoke-WebRequest ($baseUrl + $action) -WebSession $session -Method Post -Body $values
}
function Expect($response, $text) {
    if (![System.Net.WebUtility]::HtmlDecode($response.Content).Contains($text)) { throw "Expected: $text; response: $($response.Content)" }
    Write-Output "PASS: $text"
}
$r = Submit '/Account/Login' '/Account/Login' @{Email='admin@gmail.com'; Password='741852963'}
Expect $r 'Visão geral'
$r = Submit '/Materials/CreateAccessory' '/Materials/CreateAccessory' @{Nome='Pomel UX'; PesoUnitarioGramas='25'}
Expect $r 'Acessório criado com sucesso.'
$r = Submit '/Materials/CreateDisk' '/Materials/CreateDisk' @{DiametroMm='200'; GrossuraMm='1'; PesoUnitarioKg='0,1'; CalcularPesoAutomaticamente='false'}
Expect $r 'Disco criado com sucesso.'
$r = Submit '/Items/Create' '/Items/Create' @{Nome='Caneco UX'; Numero='12'; DiscoId='1'; PossuiTampa='false'; PrecoVarejo='20,50'; 'Acessorios[0].AcessorioId'='1'; 'Acessorios[0].Selecionado'='true'; 'Acessorios[0].Quantidade'='1'}
Expect $r 'Produto criado com sucesso.'
$r = Submit '/Estoque' '/Estoque/Movimentar' @{'Form.Tipo'='disco'; 'Form.Operacao'='entrada'; 'Form.DiscoId'='99999'; 'Form.QuantidadeKg'='1'}
Expect $r 'Selecione um disco valido.'
if ($r.Content -notmatch 'data-valmsg-for="Form.DiscoId"[^>]*>[^<]*Selecione') { throw 'Stock error is not next to the disk field.' }
$r = Submit '/Estoque' '/Estoque/Movimentar' @{'Form.Tipo'='disco'; 'Form.Operacao'='entrada'; 'Form.DiscoId'='1'; 'Form.QuantidadeKg'='1'}
Expect $r 'registrada com sucesso.'
$r = Submit '/Estoque' '/Estoque/Movimentar' @{'Form.Tipo'='acessorio'; 'Form.Operacao'='entrada'; 'Form.AcessorioId'='1'; 'Form.QuantidadeKg'='1'}
Expect $r 'registrada com sucesso.'
$r = Submit '/Estoque' '/Estoque/Movimentar' @{'Form.Tipo'='item'; 'Form.Operacao'='entrada'; 'Form.ItemId'='1'; 'Form.QuantidadeUnidades'='2'}
Expect $r 'consumo automatico dos componentes.'
Expect $r '0,8 kg'
Expect $r '0,95 kg'
$r = Submit '/Pedidos/Create' '/Pedidos/CreateClient' @{'NovoCliente.Nome'='Cliente UX'; 'NovoCliente.Endereco'='Rua de Teste'; 'NovoCliente.Telefone'='11999990000'; 'NovoCliente.TabelaPreco'='Varejo'}
$client = $r.Content | ConvertFrom-Json
if (!$client.success) { throw $r.Content }
$r = Submit '/Pedidos/Create' '/Pedidos/Create' @{ClienteId=$client.client.id; DataPedido='2026-09-20'; Status='Em negociacao'; 'Itens[0].ItemId'='1'; 'Itens[0].Quantidade'='2'; 'Itens[0].ValorUnitario'='20,50'}
Expect $r 'Pedido criado com sucesso.'
Expect $r '41,00'
$r = Submit '/Items' '/Items/Delete' @{id='1'}
Expect $r 'não pode ser excluído'
$r = Submit '/Materials' '/Materials/DeleteAccessory' @{id='1'}
Expect $r 'não pode ser excluído'
$r = Submit '/Materials' '/Materials/DeleteDisk' @{id='1'}
Expect $r 'não pode ser excluído'
Write-Output 'UX smoke workflow passed on isolated database.'

$r = Submit '/AdminUsers/Create' '/AdminUsers/Create' @{Nome='Operador UX'; Email='operador-ux@example.test'; Password='TesteUX123!'; NivelAcessoId='2'}
Expect $r 'Usuário criado com sucesso.'
$r = Submit '/AdminUsers/Create' '/AdminUsers/Create' @{Nome='Inválido UX'; Email='invalido-ux@example.test'; Password='TesteUX123!'; NivelAcessoId='99999'}
Expect $r 'Selecione um nível de acesso válido.'
$r = Submit '/AdminUsers/Edit/1' '/AdminUsers/Edit' @{Id='1'; Nome='admin'; Email='admin@gmail.com'; NivelAcessoId='2'}
Expect $r 'Mantenha pelo menos um administrador'
$r = Submit '/Estoque' '/Estoque/Movimentar' @{'Form.Tipo'='item'; 'Form.Operacao'='saida'; 'Form.ItemId'='1'; 'Form.QuantidadeUnidades'='999'}
Expect $r 'Saldo insuficiente.'
$r = Submit '/Pedidos' '/Pedidos/UpdateStatus' @{id='1'; status='Em producao'}
Expect $r 'Status do pedido atualizado com sucesso.'
Expect $r 'Em produção'
$r = Submit '/Account/Login' '/Account/Logout' @{}
Expect $r 'Bem-vindo à fábrica'
$r = Submit '/Account/Login' '/Account/Login' @{Email='operador-ux@example.test'; Password='TesteUX123!'}
Expect $r 'Visão geral'
if ($r.Content.Contains('>CADASTROS<')) { throw 'Operator can see admin navigation.' }
foreach ($path in @('/AdminUsers', '/Materials', '/Items')) {
    $r = Page $path
    Expect $r 'DIA A DIA DA FÁBRICA'
}
foreach ($path in @('/Pedidos', '/Estoque')) {
    $r = Page $path
    if ($r.StatusCode -ne 200) { throw "Operator cannot open $path" }
}
$r = Submit '/Account/Login' '/Account/Logout' @{}
$r = Submit '/Account/Login' '/Account/Register' @{UserName='Acesso indevido'; Email='blocked@example.test'; Password='TesteUX123!'}
Expect $r 'Bem-vindo à fábrica'
Write-Output 'User management and access checks passed.'
