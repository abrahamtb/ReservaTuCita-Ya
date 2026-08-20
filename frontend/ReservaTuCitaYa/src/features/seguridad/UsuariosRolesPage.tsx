import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { listOrganizations } from '../../api/organizacionesApi'
import { guardarPermisosRol, listarPermisos, listarRoles, obtenerPermisosRol, type PermisoDto } from '../../api/rolesApi'
import { asignarRolUsuario, cambiarEstadoUsuario, crearUsuario, listarUsuarios, type CrearUsuarioRequest, type UsuarioDto } from '../../api/usuariosApi'
import { useAuth } from '../../auth/useAuth'
import { listarClientesOrganizacion, type ClienteOpcion } from '../../api/clientesSeleccionApi'
import { listarProfesionales } from '../../api/empleadosApi'
import type { EmpleadoLista } from '../../types'
import type { Organization } from '../../types'

type Tab = 'usuarios' | 'roles'

const emptyUser: CrearUsuarioRequest = {
  email: '', password: '', nombres: '', apellidos: '', numeroDocumento: '', telefono: '', rol: 'Recepcionista', organizacionId: null, clienteId: null, empleadoId: null,
}

const moduloLabel: Record<string, string> = {
  organizaciones: 'Organización', sedes: 'Sedes', clientes: 'Clientes', empleados: 'Empleados y profesionales',
  servicios: 'Servicios', recursos: 'Recursos', horarios: 'Horarios', reservas: 'Reservas', atenciones: 'Atenciones',
  pagos: 'Pagos', dashboard: 'Dashboard', reportes: 'Reportes', calificaciones: 'Calificaciones', usuarios: 'Usuarios', roles: 'Roles',
}

export function UsuariosRolesPage() {
  const { user } = useAuth()
  const superAdmin = user?.roles.includes('Superadministrador') ?? false
  const canManageUsers = user?.permisos.includes('usuarios.gestionar') ?? false
  const canManageRoles = user?.permisos.includes('roles.gestionar') ?? false
  const [tab, setTab] = useState<Tab>('usuarios')
  const [usuarios, setUsuarios] = useState<UsuarioDto[]>([])
  const [roles, setRoles] = useState<string[]>([])
  const [permisos, setPermisos] = useState<PermisoDto[]>([])
  const [organizaciones, setOrganizaciones] = useState<Organization[]>([])
  const [clientes, setClientes] = useState<ClienteOpcion[]>([])
  const [profesionales, setProfesionales] = useState<EmpleadoLista[]>([])
  const [rolSeleccionado, setRolSeleccionado] = useState('Administrador')
  const [permisosRol, setPermisosRol] = useState<string[]>([])
  const [busqueda, setBusqueda] = useState('')
  const [filtroRol, setFiltroRol] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [showNew, setShowNew] = useState(false)
  const [nuevo, setNuevo] = useState<CrearUsuarioRequest>({ ...emptyUser, organizacionId: user?.organizacion?.id ?? null })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  async function reloadUsers() { setUsuarios(await listarUsuarios()) }

  useEffect(() => {
    const controller = new AbortController()
    setLoading(true)
    Promise.all([
      listarUsuarios(controller.signal),
      listarRoles(controller.signal),
      listarPermisos(controller.signal),
      superAdmin
        ? listOrganizations({ estado: 'Activos', pagina: 1, tamanoPagina: 100 }, controller.signal).then(result => result.elementos)
        : Promise.resolve(user?.organizacion ? [{ id: user.organizacion.id, nombreComercial: user.organizacion.nombre, tipoOrganizacion: '', numeroDocumento: '', estaActivo: true } as Organization] : []),
    ]).then(([users, roleItems, permissionItems, organizationItems]) => {
      setUsuarios(users); setRoles(roleItems); setPermisos(permissionItems); setOrganizaciones(organizationItems)
      const firstRole = roleItems.includes('Administrador') ? 'Administrador' : roleItems[0] ?? ''
      setRolSeleccionado(firstRole)
      setNuevo(current => ({ ...current, rol: roleItems.includes('Recepcionista') ? 'Recepcionista' : roleItems[0] ?? '', organizacionId: current.organizacionId ?? organizationItems[0]?.id ?? null }))
    }).catch(caught => {
      if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudo cargar seguridad.')
    }).finally(() => { if (!controller.signal.aborted) setLoading(false) })
    return () => controller.abort()
  }, [superAdmin, user?.organizacion])

  useEffect(() => {
    if (nuevo.rol !== 'Cliente' || !nuevo.organizacionId) { setClientes([]); return }
    const controller = new AbortController()
    listarClientesOrganizacion(nuevo.organizacionId, '', controller.signal)
      .then(result => setClientes(result.elementos))
      .catch(() => { if (!controller.signal.aborted) setClientes([]) })
    return () => controller.abort()
  }, [nuevo.organizacionId, nuevo.rol])

  useEffect(() => {
    if (nuevo.rol !== 'Profesional' || !nuevo.organizacionId) { setProfesionales([]); return }
    const controller = new AbortController()
    listarProfesionales(nuevo.organizacionId, controller.signal)
      .then(result => setProfesionales(result.elementos))
      .catch(() => { if (!controller.signal.aborted) setProfesionales([]) })
    return () => controller.abort()
  }, [nuevo.organizacionId, nuevo.rol])

  useEffect(() => {
    if (!rolSeleccionado) { setPermisosRol([]); return }
    const controller = new AbortController()
    obtenerPermisosRol(rolSeleccionado, controller.signal).then(setPermisosRol).catch(caught => {
      if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : 'No se pudieron cargar los permisos del rol.')
    })
    return () => controller.abort()
  }, [rolSeleccionado])

  const usuariosFiltrados = useMemo(() => usuarios.filter(item => {
    const texto = `${item.nombres} ${item.apellidos} ${item.email}`.toLowerCase()
    return (!busqueda || texto.includes(busqueda.toLowerCase()))
      && (!filtroRol || item.roles.includes(filtroRol))
      && (!filtroEstado || String(item.estaActivo) === filtroEstado)
  }), [busqueda, filtroEstado, filtroRol, usuarios])

  const grupos = useMemo(() => {
    const result = new Map<string, PermisoDto[]>()
    permisos.forEach(permiso => {
      const modulo = permiso.codigo.split('.')[0]
      const items = result.get(modulo) ?? []
      items.push(permiso); result.set(modulo, items)
    })
    return [...result.entries()]
  }, [permisos])

  async function submitNew(event: FormEvent) {
    event.preventDefault(); setSaving(true); setError(''); setSuccess('')
    try {
      await crearUsuario({ ...nuevo, organizacionId: nuevo.rol === 'Superadministrador' ? null : nuevo.organizacionId, clienteId: nuevo.rol === 'Cliente' ? nuevo.clienteId : null, empleadoId: nuevo.rol === 'Profesional' ? nuevo.empleadoId : null })
      await reloadUsers(); setShowNew(false); setNuevo({ ...emptyUser, rol: roles.includes('Recepcionista') ? 'Recepcionista' : roles[0] ?? '', organizacionId: user?.organizacion?.id ?? organizaciones[0]?.id ?? null })
      setSuccess(nuevo.rol === 'Cliente' ? 'Cuenta de cliente creada y vinculada correctamente.' : nuevo.rol === 'Profesional' ? 'Cuenta profesional creada y vinculada correctamente.' : 'Usuario creado correctamente.')
    } catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo crear el usuario.') }
    finally { setSaving(false) }
  }

  async function changeRole(id: string, rol: string) {
    setError(''); setSuccess('')
    try { await asignarRolUsuario(id, rol); await reloadUsers(); setSuccess('Rol actualizado.') }
    catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo cambiar el rol.') }
  }

  async function toggleUser(id: string) {
    setError(''); setSuccess('')
    try { await cambiarEstadoUsuario(id); await reloadUsers(); setSuccess('Estado del usuario actualizado.') }
    catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudo cambiar el estado.') }
  }

  function togglePermission(code: string) {
    setPermisosRol(current => current.includes(code) ? current.filter(item => item !== code) : [...current, code])
  }

  async function savePermissions() {
    setSaving(true); setError(''); setSuccess('')
    try { await guardarPermisosRol(rolSeleccionado, permisosRol); setSuccess('Permisos del rol guardados correctamente.') }
    catch (caught) { setError(caught instanceof Error ? caught.message : 'No se pudieron guardar los permisos.') }
    finally { setSaving(false) }
  }

  if (loading) return <div className="py-5 text-center">Cargando seguridad…</div>

  return <section>
    <div className="d-flex justify-content-between align-items-start mb-3"><div><h1>Usuarios y roles</h1><p className="text-secondary">Cuentas de acceso, roles y permisos del sistema.</p></div></div>
    {error && <div className="alert alert-danger">{error}</div>}
    {success && <div className="alert alert-success">{success}</div>}
    <ul className="nav nav-tabs mb-3">
      <li className="nav-item"><button className={`nav-link ${tab === 'usuarios' ? 'active' : ''}`} onClick={() => setTab('usuarios')}>Usuarios</button></li>
      <li className="nav-item"><button className={`nav-link ${tab === 'roles' ? 'active' : ''}`} onClick={() => setTab('roles')}>Roles y permisos</button></li>
    </ul>

    {tab === 'usuarios' && <>
      <div className="card card-body mb-3"><div className="row g-2 align-items-end">
        <div className="col-lg-5"><label className="form-label">Buscar usuario</label><input className="form-control" placeholder="Nombre o correo" value={busqueda} onChange={event => setBusqueda(event.target.value)} /></div>
        <div className="col-lg-2"><label className="form-label">Rol</label><select className="form-select" value={filtroRol} onChange={event => setFiltroRol(event.target.value)}><option value="">Todos</option>{roles.map(role => <option key={role}>{role}</option>)}</select></div>
        <div className="col-lg-2"><label className="form-label">Estado</label><select className="form-select" value={filtroEstado} onChange={event => setFiltroEstado(event.target.value)}><option value="">Todos</option><option value="true">Activos</option><option value="false">Inactivos</option></select></div>
        <div className="col-lg-3 text-lg-end"><button className="btn btn-primary" disabled={!canManageUsers} onClick={() => setShowNew(value => !value)}>+ Nuevo usuario</button></div>
      </div></div>

      {showNew && <form className="card card-body mb-3" onSubmit={submitNew}><h2 className="h5">Nuevo usuario</h2><div className="row g-3">
        <div className="col-md-6"><label className="form-label">Nombres *</label><input required className="form-control" value={nuevo.nombres} onChange={e => setNuevo({ ...nuevo, nombres: e.target.value })} /></div>
        <div className="col-md-6"><label className="form-label">Apellidos *</label><input required className="form-control" value={nuevo.apellidos} onChange={e => setNuevo({ ...nuevo, apellidos: e.target.value })} /></div>
        <div className="col-md-6"><label className="form-label">Correo *</label><input required type="email" className="form-control" value={nuevo.email} onChange={e => setNuevo({ ...nuevo, email: e.target.value })} /></div>
        <div className="col-md-6"><label className="form-label">Contraseña *</label><input required minLength={8} type="password" className="form-control" value={nuevo.password} onChange={e => setNuevo({ ...nuevo, password: e.target.value })} /></div>
        <div className="col-md-4"><label className="form-label">Documento *</label><input required className="form-control" value={nuevo.numeroDocumento} onChange={e => setNuevo({ ...nuevo, numeroDocumento: e.target.value })} /></div>
        <div className="col-md-4"><label className="form-label">Teléfono *</label><input required className="form-control" value={nuevo.telefono} onChange={e => setNuevo({ ...nuevo, telefono: e.target.value })} /></div>
        <div className="col-md-4"><label className="form-label">Rol *</label><select required className="form-select" value={nuevo.rol} onChange={e => setNuevo({ ...nuevo, rol: e.target.value, clienteId: null, empleadoId: null })}>{roles.filter(role => superAdmin || role !== 'Superadministrador').map(role => <option key={role}>{role}</option>)}</select></div>
        {nuevo.rol !== 'Superadministrador' && <div className="col-md-6"><label className="form-label">Organización *</label><select required className="form-select" value={nuevo.organizacionId ?? ''} disabled={!superAdmin} onChange={e => setNuevo({ ...nuevo, organizacionId: e.target.value })}>{organizaciones.map(org => <option key={org.id} value={org.id}>{org.nombreComercial}</option>)}</select></div>}
        {nuevo.rol === 'Cliente' && <div className="col-md-6"><label className="form-label">Cliente vinculado *</label><select required className="form-select" value={nuevo.clienteId ?? ''} onChange={e => setNuevo({ ...nuevo, clienteId: e.target.value || null })}><option value="">Selecciona un cliente</option>{clientes.map(cliente => <option key={cliente.id} value={cliente.id}>{cliente.nombreCompleto} · {cliente.numeroDocumento}</option>)}</select><small className="text-secondary">Esta cuenta solo podrá ver y administrar sus propias reservas.</small></div>}
        {nuevo.rol === 'Profesional' && <div className="col-md-6"><label className="form-label">Profesional vinculado *</label><select required className="form-select" value={nuevo.empleadoId ?? ''} onChange={e => setNuevo({ ...nuevo, empleadoId: e.target.value || null })}><option value="">Selecciona un profesional</option>{profesionales.map(profesional => <option key={profesional.id} value={profesional.id}>{profesional.nombreCompleto} · {profesional.especialidad || profesional.cargo}</option>)}</select><small className="text-secondary">Esta cuenta solo podrá consultar y atender su propia agenda.</small></div>}
      </div><div className="mt-3 d-flex gap-2"><button className="btn btn-primary" disabled={saving}>{saving ? 'Guardando…' : 'Guardar usuario'}</button><button type="button" className="btn btn-outline-secondary" onClick={() => setShowNew(false)}>Cancelar</button></div></form>}

      <div className="card table-responsive"><table className="table table-hover align-middle mb-0"><thead><tr><th>Nombre</th><th>Correo</th><th>Rol</th><th>Organización</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>{usuariosFiltrados.map(item => <tr key={item.id}>
        <td>{item.nombres} {item.apellidos}</td><td>{item.email}</td><td><select className="form-select form-select-sm" disabled={!canManageUsers} value={item.roles[0] ?? ''} onChange={e => void changeRole(item.id, e.target.value)}>{roles.filter(role => superAdmin || role !== 'Superadministrador').map(role => <option key={role}>{role}</option>)}</select></td>
        <td>{organizaciones.find(org => org.id === item.organizacionId)?.nombreComercial ?? (item.organizacionId ? 'Organización asignada' : 'Global')}</td>
        <td><span className={`badge ${item.estaActivo ? 'text-bg-success' : 'text-bg-secondary'}`}>{item.estaActivo ? 'Activo' : 'Inactivo'}</span></td>
        <td><button className={`btn btn-sm ${item.estaActivo ? 'btn-outline-danger' : 'btn-outline-success'}`} disabled={!canManageUsers} onClick={() => void toggleUser(item.id)}>{item.estaActivo ? 'Desactivar' : 'Activar'}</button></td>
      </tr>)}</tbody></table></div>
    </>}

    {tab === 'roles' && <div className="row g-3">
      <div className="col-lg-3"><div className="list-group">{roles.map(role => <button key={role} type="button" className={`list-group-item list-group-item-action ${rolSeleccionado === role ? 'active' : ''}`} onClick={() => setRolSeleccionado(role)}>{role}</button>)}</div></div>
      <div className="col-lg-9"><div className="card card-body"><div className="d-flex justify-content-between align-items-center mb-3"><div><h2 className="h5 mb-1">{rolSeleccionado}</h2><span className="text-secondary">Permisos agrupados por módulo</span></div><button className="btn btn-primary" disabled={!canManageRoles || saving || (rolSeleccionado === 'Superadministrador' && !superAdmin)} onClick={() => void savePermissions()}>{saving ? 'Guardando…' : 'Guardar permisos'}</button></div>
        <div className="row g-3">{grupos.map(([modulo, items]) => <div className="col-md-6" key={modulo}><div className="border rounded p-3 h-100"><h3 className="h6 text-uppercase">{moduloLabel[modulo] ?? modulo}</h3>{items.map(item => <div className="form-check" key={item.codigo}><input className="form-check-input" type="checkbox" id={`perm-${item.id}`} checked={permisosRol.includes(item.codigo)} disabled={!canManageRoles || (rolSeleccionado === 'Superadministrador' && !superAdmin)} onChange={() => togglePermission(item.codigo)} /><label className="form-check-label" htmlFor={`perm-${item.id}`}>{item.nombre || item.codigo.split('.')[1]}</label></div>)}</div></div>)}</div>
        <div className="alert alert-light border mt-3 mb-0">El menú se oculta según permisos, pero el backend vuelve a validar la autorización en cada operación.</div>
      </div></div>
    </div>}
  </section>
}
