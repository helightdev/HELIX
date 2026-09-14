import React, { useEffect, useRef, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { ReactFlow, Background, Controls, MiniMap, Handle, Position, applyNodeChanges, useUpdateNodeInternals } from '@xyflow/react';
import '@xyflow/react/dist/style.css';
import './style.css';

async function rpc(method, params = {}) {
  const response = await fetch('/api', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ method, params }) });
  const data = await response.json();
  if (data.error) throw new Error(data.error.message);
  return data.result;
}
function AssetNode({ id, data, selected }) {
  const update = useUpdateNodeInternals();
  useEffect(() => { update(id); }, [id, data.ports.length, update]);
  return <div className={`asset-node ${selected ? 'selected' : ''}`}>
    <header><small>{data.kind.toUpperCase()}</small><strong>{data.type}</strong><Handle type="source" position={Position.Right} id="value" /></header>
    {data.sectionId && <div className="anchor" title={data.sectionId}>&{data.sectionId}</div>}
    {data.ports.map(p => <div className="port" key={p.id}><Handle type="target" position={Position.Left} id={p.id} />
      <span>{p.name === '+' ? '+ Add item' : p.name}</span><em>{p.type}</em></div>)}
  </div>;
}
const nodeTypes = { asset: AssetNode };
function Field({ port, edit, remove, disabled }) {
  const [value, setValue] = useState(port.text ?? '');
  useEffect(() => setValue(port.text ?? ''), [port.text]);
  return <form onSubmit={event => { event.preventDefault(); edit(value); }}>
    <label>{port.name === '+' ? 'Append list item' : port.name}<small>{port.type}{port.hasDefault ? ' · default' : port.optional ? ' · optional' : ' · required'}</small></label>
    <div className="field-input"><input aria-label={port.name} value={value} placeholder="HAL value" onChange={e => setValue(e.target.value)} disabled={disabled} /><button disabled={disabled || !value.trim()}>Apply</button></div>{remove && <button type="button" disabled={disabled} onClick={remove}>Remove item</button>}
  </form>;
}
function App() {
  const [doc, setDoc] = useState(null), [nodes, setNodes] = useState([]), [files, setFiles] = useState(null);
  const [selected, setSelected] = useState(null), [error, setError] = useState(''), [busy, setBusy] = useState(false);
  const [path, setPath] = useState(''), [type, setType] = useState(''), [source, setSource] = useState(''), [showSource, setShowSource] = useState(false);
  const flow = useRef(null), connecting = useRef(null);
  const [menu, setMenu] = useState(null), [search, setSearch] = useState(''), [fieldName, setFieldName] = useState('');
  function install(next, placed = null) {
    setDoc(next); setSource(next.text);
    setNodes(previous => next.nodes.map((node, index) => ({ id: node.id, type: 'asset', data: node,
      position: node.position ?? previous.find(n => n.id === node.id)?.position ?? placed ?? { x: (index % 3) * 340, y: Math.floor(index / 3) * 320 } })));
  }
  async function perform(action) { setBusy(true); setError(''); try { await action(); } catch (e) { setError(e.message); } finally { setBusy(false); } }
  async function browse(directory) { const next = await rpc('files/list', { path: directory }); setFiles(next); }
  useEffect(() => { perform(() => browse()); }, []);
  useEffect(() => { const warn = e => { if (doc?.dirty || source !== doc?.text && doc) { e.preventDefault(); e.returnValue = ''; } }; window.addEventListener('beforeunload', warn); return () => window.removeEventListener('beforeunload', warn); }, [doc, source]);
  async function open(file) {
    if ((doc?.dirty || doc && source !== doc.text) && !window.confirm('Discard unsaved changes and open another asset?')) return;
    const next = await rpc('hal/open', { path: file });
    if (doc) await rpc('hal/close', { session: doc.session, revision: doc.revision });
    setSelected(null); setNodes([]); install(next); setPath(file);
  }
  function command(method, extra = {}) { return perform(async () => install(await rpc(method, { session: doc.session, revision: doc.revision, ...extra }))); }
  function save() { return perform(async () => {
    const laidOut = await rpc('hal/edit', { session: doc.session, revision: doc.revision, edit: { operation: 'layout', positions: nodes.map(n => ({ node: n.id, x: n.position.x, y: n.position.y })) } });
    install(laidOut);
    install(await rpc('hal/save', { session: laidOut.session, revision: laidOut.revision }));
  }); }
  function edit(operation, extra = {}) { return command('hal/edit', { edit: { operation, ...extra } }); }
  async function offerNodes(event, state) {
    const from = connecting.current; connecting.current = null;
    if (!from || state.isValid || locked || !event.target.closest('.react-flow__pane')) return;
    const point = event.changedTouches?.[0] ?? event;
    const direction = from.handleType === 'target' ? 'supplier' : 'receiver';
    const request = { session: doc.session, revision: doc.revision, node: from.nodeId, port: from.handleId, direction };
    setSearch('');
    await perform(async () => {
      const choices = await rpc('hal/candidates', request);
      setMenu({ ...request, choices, x: Math.min(point.clientX, window.innerWidth - 310), y: Math.min(point.clientY, window.innerHeight - 360),
        position: flow.current.screenToFlowPosition({ x: point.clientX, y: point.clientY }) });
    });
  }
  function createConnected(choice) {
    const pending = menu; setMenu(null);
    return perform(async () => {
      const next = await rpc('hal/edit', { session: pending.session, revision: pending.revision,
        edit: { operation: 'createConnected', node: pending.node, port: pending.port, direction: pending.direction, value: choice.type, receiverPort: choice.port, x: pending.position.x, y: pending.position.y } });
      const created = next.nodes.find(n => !doc.nodes.some(old => old.id === n.id));
      install(next, pending.position); setSelected(created?.id ?? null);
    });
  }
  useEffect(() => { const escape = e => { if (e.key === 'Escape') setMenu(null); }; window.addEventListener('keydown', escape); return () => window.removeEventListener('keydown', escape); }, []);
  const node = doc?.nodes.find(n => n.id === selected);
  const locked = busy || !!doc && source !== doc.text;
  const edges = (doc?.edges ?? []).map(e => ({ id: e.id, source: e.source, sourceHandle: 'value', target: e.target, targetHandle: e.targetPort, type: 'smoothstep' }));
  return <main>
    <div className="toolbar"><div className="brand">HAL <span>GRAPH EDITOR</span></div>
      <form className="open" onSubmit={e => { e.preventDefault(); perform(() => open(path)); }}><input aria-label="Asset file path" value={path} onChange={e => setPath(e.target.value)} placeholder="Server path to an asset.hal" /><button disabled={busy || !path}>Open</button></form>
      <button disabled={!doc || locked} onClick={save}>Save{doc?.dirty ? ' •' : ''}</button>
      <button disabled={locked || !doc?.canUndo} onClick={() => command('hal/undo')}>Undo</button><button disabled={locked || !doc?.canRedo} onClick={() => command('hal/redo')}>Redo</button>
      <button disabled={!doc || locked} onClick={() => command('hal/get')}>Refresh types</button>
      <button onClick={() => setShowSource(!showSource)}>{showSource ? 'Graph' : 'Source'}</button>
    </div>
    {error && <div className="error" role="alert">{error}</div>}
    <div className="workspace"><aside className="browser"><h3>Asset files</h3><div className="directory">{files?.path}</div>
      {files && files.path !== files.root && <button disabled={busy} onClick={() => perform(() => browse(files.path.replace(/[\\/][^\\/]+$/, '')))}>↑ Parent directory</button>}
      {files?.entries.map(entry => <button disabled={busy} className="file" key={entry.path} onClick={() => perform(() => entry.directory ? browse(entry.path) : open(entry.path))}>{entry.directory ? '▸ ' : '◇ '}{entry.name}</button>)}
    </aside>
    <section className="canvas">{!doc ? <div className="empty"><h1>Build your asset graph</h1><p>Open a HAL file from the server to inspect objects, lists, and references.</p></div> : showSource ? <div className="source-editor"><textarea aria-label="HAL source" spellCheck={false} value={source} onChange={e => setSource(e.target.value)} /><button disabled={busy || source === doc.text} onClick={() => command('hal/change', { text: source })}>Apply source changes</button></div> : <ReactFlow colorMode="dark" nodes={nodes} edges={edges} nodeTypes={nodeTypes} fitView
      onNodesChange={changes => setNodes(current => applyNodeChanges(changes, current))}
      onNodeClick={(_, n) => setSelected(n.id)} onPaneClick={() => { setSelected(null); setMenu(null); }} nodesConnectable={!locked} nodesDraggable={!locked} nodesDeletable={false} edgesReconnectable={false} deleteKeyCode={null}
      onNodeDragStop={(_, moved, group) => edit('layout', { positions: (group?.length ? group : [moved]).map(n => ({ node: n.id, x: n.position.x, y: n.position.y })) })}
      onInit={instance => { flow.current = instance; }}
      onConnectStart={(_, handle) => { connecting.current = handle; setMenu(null); }}
      onConnectEnd={offerNodes}
      onConnect={connection => edit('connect', { source: connection.source, node: connection.target, port: connection.targetHandle })}
      onEdgeDoubleClick={(_, edge) => !locked && edit('disconnect', { node: edge.target, port: edge.targetHandle })}>
      <Background gap={24} /><Controls /><MiniMap /></ReactFlow>}</section>
    <aside className="inspector"><h3>Inspector</h3>{doc && <><label>Create section</label><div className="create-primitives"><button disabled={locked} onClick={() => edit('add', { value: 'table' })}>+ Table</button><button disabled={locked} onClick={() => edit('add', { value: 'tuple' })}>+ List</button></div><div className="field-input"><select aria-label="Section type" value={type} onChange={e => setType(e.target.value)}><option value="">Choose type…</option>{doc.types.filter(t => t.name !== 'table' && t.name !== 'tuple').map(t => <option key={t.name}>{t.name}</option>)}</select><button disabled={locked || !type} onClick={() => edit('add', { value: type })}>Add</button></div></>}
      {node ? <><h2>{node.type}</h2>{node.ports.map(p => <Field key={`${node.id}/${p.id}`} port={p} disabled={locked} remove={node.kind === 'list' && p.id !== '+' ? () => edit('removeItem', { node: node.id, port: p.id }) : undefined} edit={value => edit('set', { node: node.id, port: p.id, value })} />)}
        {node.kind === 'table' && <form onSubmit={event => { event.preventDefault(); edit('addField', { node: node.id, port: fieldName, value: 'null' }); setFieldName(''); }}><label>New table field</label><div className="field-input"><input aria-label="New table field" value={fieldName} onChange={e => setFieldName(e.target.value)} placeholder="Field name" /><button disabled={locked || !fieldName.trim()}>Add field</button></div></form>}
        {<button className="danger" disabled={locked} onClick={() => edit('remove', { node: node.id })}>Remove section</button>}</> : <p>Select a node to edit its fields. Values use HAL syntax, for example <code>42</code>, <code>"hello"</code>, or <code>[]</code>.</p>}
      <p className="hint">Drag an output onto a field port to connect. Double-click an edge to set its field to null. Drop a wire on empty space to create a compatible supplier or receiver. Every node is a section and every wire is a reference.</p>
    </aside></div>
    {menu && <div className="creation-menu" role="dialog" aria-label="Create connected section" style={{ left: menu.x, top: menu.y }}>
      <div className="creation-title"><strong>Create {menu.direction}</strong><button aria-label="Cancel creation" onClick={() => setMenu(null)}>×</button></div>
      <input autoFocus aria-label="Filter node types" placeholder="Filter types…" value={search} onChange={e => setSearch(e.target.value)} />
      <div className="creation-options">{menu.choices.filter(c => c.label.toLowerCase().includes(search.toLowerCase())).map(c => <button key={`${c.type}/${c.port}`} disabled={busy} onClick={() => createConnected(c)}>{c.label}<small>{c.kind} section</small></button>)}
      {!menu.choices.length && <p>No compatible section types are available for this port.</p>}</div>
    </div>}
    <footer><span>{busy ? 'Working…' : doc ? `Revision ${doc.revision} · ${doc.dirty ? 'Unsaved' : 'Saved'}` : 'Ready'}</span><span>Workspace: {doc?.workspace ?? 'Open an asset'}</span></footer>
    {!!doc?.diagnostics.length && <div className="diagnostics" role="status">{doc.diagnostics.map((d, i) => <div key={i}>{d}</div>)}</div>}
  </main>;
}
createRoot(document.getElementById('root')).render(<App />);
