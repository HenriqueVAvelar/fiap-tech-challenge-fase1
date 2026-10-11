using AutoMapper;
using Oficina.Application.DTOs.Usuario;
using Oficina.Application.Interfaces;
using Oficina.Domain.Entities;
using Oficina.Domain.Interfaces;

namespace Oficina.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public UsuarioService(IUsuarioRepository repository, IPasswordHasher passwordHasher, IMapper mapper)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    #region Basic CRUD
    public async Task<IEnumerable<UsuarioDto>> ObterTodosAsync()
    {
        var usuarios = await _repository.ObterTodosAsync();

        return _mapper.Map<IEnumerable<UsuarioDto>>(usuarios);
    }

    public async Task<UsuarioDto?> ObterPorIdAsync(int id)
    {
        var usuario = await _repository.ObterPorIdAsync(id);

        return usuario is null ? null : _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<UsuarioDto> CriarAsync(UsuarioInputDto dto)
    {
        Usuario.ValidarSenha(dto.Senha);
        await GarantirUserNameDisponivelAsync(dto.UserName);

        var usuario = new Usuario(
            dto.Nome,
            dto.UserName,
            _passwordHasher.Hash(dto.Senha),
            dto.Perfil!.Value);

        await _repository.AdicionarAsync(usuario);

        return _mapper.Map<UsuarioDto>(usuario);
    }

    public async Task<bool> AtualizarAsync(int id, UsuarioUpdateInputDto dto)
    {
        var usuario = await _repository.ObterPorIdAsync(id);

        if (usuario is null)
            return false;

        await GarantirUserNameDisponivelAsync(dto.UserName, id);

        usuario.Atualizar(dto.Nome, dto.UserName, dto.Perfil!.Value);

        if (!string.IsNullOrEmpty(dto.Senha))
        {
            Usuario.ValidarSenha(dto.Senha);
            usuario.AlterarSenha(_passwordHasher.Hash(dto.Senha));
        }

        await _repository.AtualizarAsync(usuario);

        return true;
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var usuario = await _repository.ObterPorIdAsync(id);

        if (usuario is null)
            return false;

        await _repository.RemoverAsync(id);

        return true;
    }
    #endregion

    private async Task GarantirUserNameDisponivelAsync(string userName, int? ignorarId = null)
    {
        var normalizado = userName.Trim();

        if (await _repository.UserNameExisteAsync(normalizado, ignorarId))
            throw new InvalidOperationException($"O UserName '{normalizado}' já está em uso.");
    }
}
