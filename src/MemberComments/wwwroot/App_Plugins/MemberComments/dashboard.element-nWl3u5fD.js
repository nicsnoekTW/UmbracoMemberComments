import { LitElement as ne, html as V, css as oe, state as j, customElement as ce } from "@umbraco-cms/backoffice/external/lit";
import { UmbElementMixin as ue } from "@umbraco-cms/backoffice/element-api";
import { UMB_NOTIFICATION_CONTEXT as le } from "@umbraco-cms/backoffice/notification";
import { UMB_CURRENT_USER_CONTEXT as de } from "@umbraco-cms/backoffice/current-user";
import { umbHttpClient as fe } from "@umbraco-cms/backoffice/http-client";
const he = {
  bodySerializer: (t) => JSON.stringify(
    t,
    (e, r) => typeof r == "bigint" ? r.toString() : r
  )
}, me = ({
  onRequest: t,
  onSseError: e,
  onSseEvent: r,
  responseTransformer: s,
  responseValidator: a,
  sseDefaultRetryDelay: u,
  sseMaxRetryAttempts: o,
  sseMaxRetryDelay: n,
  sseSleepFn: c,
  url: d,
  ...i
}) => {
  let f;
  const E = c ?? ((l) => new Promise((m) => setTimeout(m, l)));
  return { stream: async function* () {
    let l = u ?? 3e3, m = 0;
    const x = i.signal ?? new AbortController().signal;
    for (; !x.aborted; ) {
      m++;
      const _ = i.headers instanceof Headers ? i.headers : new Headers(i.headers);
      f !== void 0 && _.set("Last-Event-ID", f);
      try {
        const S = {
          redirect: "follow",
          ...i,
          body: i.serializedBody,
          headers: _,
          signal: x
        };
        let b = new Request(d, S);
        t && (b = await t(d, S));
        const p = await (i.fetch ?? globalThis.fetch)(b);
        if (!p.ok)
          throw new Error(
            `SSE failed: ${p.status} ${p.statusText}`
          );
        if (!p.body) throw new Error("No body in SSE response");
        const g = p.body.pipeThrough(new TextDecoderStream()).getReader();
        let I = "";
        const B = () => {
          try {
            g.cancel();
          } catch {
          }
        };
        x.addEventListener("abort", B);
        try {
          for (; ; ) {
            const { done: re, value: se } = await g.read();
            if (re) break;
            I += se;
            const R = I.split(`

`);
            I = R.pop() ?? "";
            for (const ae of R) {
              const ie = ae.split(`
`), O = [];
              let H;
              for (const y of ie)
                if (y.startsWith("data:"))
                  O.push(y.replace(/^data:\s*/, ""));
                else if (y.startsWith("event:"))
                  H = y.replace(/^event:\s*/, "");
                else if (y.startsWith("id:"))
                  f = y.replace(/^id:\s*/, "");
                else if (y.startsWith("retry:")) {
                  const F = Number.parseInt(
                    y.replace(/^retry:\s*/, ""),
                    10
                  );
                  Number.isNaN(F) || (l = F);
                }
              let C, L = !1;
              if (O.length) {
                const y = O.join(`
`);
                try {
                  C = JSON.parse(y), L = !0;
                } catch {
                  C = y;
                }
              }
              L && (a && await a(C), s && (C = await s(C))), r?.({
                data: C,
                event: H,
                id: f,
                retry: l
              }), O.length && (yield C);
            }
          }
        } finally {
          x.removeEventListener("abort", B), g.releaseLock();
        }
        break;
      } catch (S) {
        if (e?.(S), o !== void 0 && m >= o)
          break;
        const b = Math.min(
          l * 2 ** (m - 1),
          n ?? 3e4
        );
        await E(b);
      }
    }
  }() };
}, pe = (t) => {
  switch (t) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, ye = (t) => {
  switch (t) {
    case "form":
      return ",";
    case "pipeDelimited":
      return "|";
    case "spaceDelimited":
      return "%20";
    default:
      return ",";
  }
}, be = (t) => {
  switch (t) {
    case "label":
      return ".";
    case "matrix":
      return ";";
    case "simple":
      return ",";
    default:
      return "&";
  }
}, X = ({
  allowReserved: t,
  explode: e,
  name: r,
  style: s,
  value: a
}) => {
  if (!e) {
    const n = (t ? a : a.map((c) => encodeURIComponent(c))).join(ye(s));
    switch (s) {
      case "label":
        return `.${n}`;
      case "matrix":
        return `;${r}=${n}`;
      case "simple":
        return n;
      default:
        return `${r}=${n}`;
    }
  }
  const u = pe(s), o = a.map((n) => s === "label" || s === "simple" ? t ? n : encodeURIComponent(n) : N({
    allowReserved: t,
    name: r,
    value: n
  })).join(u);
  return s === "label" || s === "matrix" ? u + o : o;
}, N = ({
  allowReserved: t,
  name: e,
  value: r
}) => {
  if (r == null)
    return "";
  if (typeof r == "object")
    throw new Error(
      "Deeply-nested arrays/objects aren’t supported. Provide your own `querySerializer()` to handle these."
    );
  return `${e}=${t ? r : encodeURIComponent(r)}`;
}, Q = ({
  allowReserved: t,
  explode: e,
  name: r,
  style: s,
  value: a,
  valueOnly: u
}) => {
  if (a instanceof Date)
    return u ? a.toISOString() : `${r}=${a.toISOString()}`;
  if (s !== "deepObject" && !e) {
    let c = [];
    Object.entries(a).forEach(([i, f]) => {
      c = [
        ...c,
        i,
        t ? f : encodeURIComponent(f)
      ];
    });
    const d = c.join(",");
    switch (s) {
      case "form":
        return `${r}=${d}`;
      case "label":
        return `.${d}`;
      case "matrix":
        return `;${r}=${d}`;
      default:
        return d;
    }
  }
  const o = be(s), n = Object.entries(a).map(
    ([c, d]) => N({
      allowReserved: t,
      name: s === "deepObject" ? `${r}[${c}]` : c,
      value: d
    })
  ).join(o);
  return s === "label" || s === "matrix" ? o + n : n;
}, ge = /\{[^{}]+\}/g, we = ({ path: t, url: e }) => {
  let r = e;
  const s = e.match(ge);
  if (s)
    for (const a of s) {
      let u = !1, o = a.substring(1, a.length - 1), n = "simple";
      o.endsWith("*") && (u = !0, o = o.substring(0, o.length - 1)), o.startsWith(".") ? (o = o.substring(1), n = "label") : o.startsWith(";") && (o = o.substring(1), n = "matrix");
      const c = t[o];
      if (c == null)
        continue;
      if (Array.isArray(c)) {
        r = r.replace(
          a,
          X({ explode: u, name: o, style: n, value: c })
        );
        continue;
      }
      if (typeof c == "object") {
        r = r.replace(
          a,
          Q({
            explode: u,
            name: o,
            style: n,
            value: c,
            valueOnly: !0
          })
        );
        continue;
      }
      if (n === "matrix") {
        r = r.replace(
          a,
          `;${N({
            name: o,
            value: c
          })}`
        );
        continue;
      }
      const d = encodeURIComponent(
        n === "label" ? `.${c}` : c
      );
      r = r.replace(a, d);
    }
  return r;
}, ve = ({
  baseUrl: t,
  path: e,
  query: r,
  querySerializer: s,
  url: a
}) => {
  const u = a.startsWith("/") ? a : `/${a}`;
  let o = (t ?? "") + u;
  e && (o = we({ path: e, url: o }));
  let n = r ? s(r) : "";
  return n.startsWith("?") && (n = n.substring(1)), n && (o += `?${n}`), o;
};
function xe(t) {
  const e = t.body !== void 0;
  if (e && t.bodySerializer)
    return "serializedBody" in t ? t.serializedBody !== void 0 && t.serializedBody !== "" ? t.serializedBody : null : t.body !== "" ? t.body : null;
  if (e)
    return t.body;
}
const Se = async (t, e) => {
  const r = typeof e == "function" ? await e(t) : e;
  if (r)
    return t.scheme === "bearer" ? `Bearer ${r}` : t.scheme === "basic" ? `Basic ${btoa(r)}` : r;
}, Y = ({
  allowReserved: t,
  array: e,
  object: r
} = {}) => (a) => {
  const u = [];
  if (a && typeof a == "object")
    for (const o in a) {
      const n = a[o];
      if (n != null)
        if (Array.isArray(n)) {
          const c = X({
            allowReserved: t,
            explode: !0,
            name: o,
            style: "form",
            value: n,
            ...e
          });
          c && u.push(c);
        } else if (typeof n == "object") {
          const c = Q({
            allowReserved: t,
            explode: !0,
            name: o,
            style: "deepObject",
            value: n,
            ...r
          });
          c && u.push(c);
        } else {
          const c = N({
            allowReserved: t,
            name: o,
            value: n
          });
          c && u.push(c);
        }
    }
  return u.join("&");
}, Ce = (t) => {
  if (!t)
    return "stream";
  const e = t.split(";")[0]?.trim();
  if (e) {
    if (e.startsWith("application/json") || e.endsWith("+json"))
      return "json";
    if (e === "multipart/form-data")
      return "formData";
    if (["application/", "audio/", "image/", "video/"].some(
      (r) => e.startsWith(r)
    ))
      return "blob";
    if (e.startsWith("text/"))
      return "text";
  }
}, Ee = (t, e) => e ? !!(t.headers.has(e) || t.query?.[e] || t.headers.get("Cookie")?.includes(`${e}=`)) : !1, _e = async ({
  security: t,
  ...e
}) => {
  for (const r of t) {
    if (Ee(e, r.name))
      continue;
    const s = await Se(r, e.auth);
    if (!s)
      continue;
    const a = r.name ?? "Authorization";
    switch (r.in) {
      case "query":
        e.query || (e.query = {}), e.query[a] = s;
        break;
      case "cookie":
        e.headers.append("Cookie", `${a}=${s}`);
        break;
      default:
        e.headers.set(a, s);
        break;
    }
  }
}, J = (t) => ve({
  baseUrl: t.baseUrl,
  path: t.path,
  query: t.query,
  querySerializer: typeof t.querySerializer == "function" ? t.querySerializer : Y(t.querySerializer),
  url: t.url
}), G = (t, e) => {
  const r = { ...t, ...e };
  return r.baseUrl?.endsWith("/") && (r.baseUrl = r.baseUrl.substring(0, r.baseUrl.length - 1)), r.headers = K(t.headers, e.headers), r;
}, Te = (t) => {
  const e = [];
  return t.forEach((r, s) => {
    e.push([s, r]);
  }), e;
}, K = (...t) => {
  const e = new Headers();
  for (const r of t) {
    if (!r)
      continue;
    const s = r instanceof Headers ? Te(r) : Object.entries(r);
    for (const [a, u] of s)
      if (u === null)
        e.delete(a);
      else if (Array.isArray(u))
        for (const o of u)
          e.append(a, o);
      else u !== void 0 && e.set(
        a,
        typeof u == "object" ? JSON.stringify(u) : u
      );
  }
  return e;
};
class A {
  constructor() {
    this.fns = [];
  }
  clear() {
    this.fns = [];
  }
  eject(e) {
    const r = this.getInterceptorIndex(e);
    this.fns[r] && (this.fns[r] = null);
  }
  exists(e) {
    const r = this.getInterceptorIndex(e);
    return !!this.fns[r];
  }
  getInterceptorIndex(e) {
    return typeof e == "number" ? this.fns[e] ? e : -1 : this.fns.indexOf(e);
  }
  update(e, r) {
    const s = this.getInterceptorIndex(e);
    return this.fns[s] ? (this.fns[s] = r, e) : !1;
  }
  use(e) {
    return this.fns.push(e), this.fns.length - 1;
  }
}
const ke = () => ({
  error: new A(),
  request: new A(),
  response: new A()
}), $e = Y({
  allowReserved: !1,
  array: {
    explode: !0,
    style: "form"
  },
  object: {
    explode: !0,
    style: "deepObject"
  }
}), ze = {
  "Content-Type": "application/json"
}, Z = (t = {}) => ({
  ...he,
  headers: ze,
  parseAs: "auto",
  querySerializer: $e,
  ...t
}), Oe = (t = {}) => {
  let e = G(Z(), t);
  const r = () => ({ ...e }), s = (d) => (e = G(e, d), r()), a = ke(), u = async (d) => {
    const i = {
      ...e,
      ...d,
      fetch: d.fetch ?? e.fetch ?? globalThis.fetch,
      headers: K(e.headers, d.headers),
      serializedBody: void 0
    };
    i.security && await _e({
      ...i,
      security: i.security
    }), i.requestValidator && await i.requestValidator(i), i.body !== void 0 && i.bodySerializer && (i.serializedBody = i.bodySerializer(i.body)), (i.body === void 0 || i.serializedBody === "") && i.headers.delete("Content-Type");
    const f = J(i);
    return { opts: i, url: f };
  }, o = async (d) => {
    const { opts: i, url: f } = await u(d), E = {
      redirect: "follow",
      ...i,
      body: xe(i)
    };
    let w = new Request(f, E);
    for (const h of a.request.fns)
      h && (w = await h(w, i));
    const z = i.fetch;
    let l = await z(w);
    for (const h of a.response.fns)
      h && (l = await h(l, w, i));
    const m = {
      request: w,
      response: l
    };
    if (l.ok) {
      const h = (i.parseAs === "auto" ? Ce(l.headers.get("Content-Type")) : i.parseAs) ?? "json";
      if (l.status === 204 || l.headers.get("Content-Length") === "0") {
        let g;
        switch (h) {
          case "arrayBuffer":
          case "blob":
          case "text":
            g = await l[h]();
            break;
          case "formData":
            g = new FormData();
            break;
          case "stream":
            g = l.body;
            break;
          default:
            g = {};
            break;
        }
        return i.responseStyle === "data" ? g : {
          data: g,
          ...m
        };
      }
      let p;
      switch (h) {
        case "arrayBuffer":
        case "blob":
        case "formData":
        case "json":
        case "text":
          p = await l[h]();
          break;
        case "stream":
          return i.responseStyle === "data" ? l.body : {
            data: l.body,
            ...m
          };
      }
      return h === "json" && (i.responseValidator && await i.responseValidator(p), i.responseTransformer && (p = await i.responseTransformer(p))), i.responseStyle === "data" ? p : {
        data: p,
        ...m
      };
    }
    const x = await l.text();
    let _;
    try {
      _ = JSON.parse(x);
    } catch {
    }
    const S = _ ?? x;
    let b = S;
    for (const h of a.error.fns)
      h && (b = await h(S, l, w, i));
    if (b = b || {}, i.throwOnError)
      throw b;
    return i.responseStyle === "data" ? void 0 : {
      error: b,
      ...m
    };
  }, n = (d) => (i) => o({ ...i, method: d }), c = (d) => async (i) => {
    const { opts: f, url: E } = await u(i);
    return me({
      ...f,
      body: f.body,
      headers: f.headers,
      method: d,
      onRequest: async (w, z) => {
        let l = new Request(w, z);
        for (const m of a.request.fns)
          m && (l = await m(l, f));
        return l;
      },
      url: E
    });
  };
  return {
    buildUrl: J,
    connect: n("CONNECT"),
    delete: n("DELETE"),
    get: n("GET"),
    getConfig: r,
    head: n("HEAD"),
    interceptors: a,
    options: n("OPTIONS"),
    patch: n("PATCH"),
    post: n("POST"),
    put: n("PUT"),
    request: o,
    setConfig: s,
    sse: {
      connect: c("CONNECT"),
      delete: c("DELETE"),
      get: c("GET"),
      head: c("HEAD"),
      options: c("OPTIONS"),
      patch: c("PATCH"),
      post: c("POST"),
      put: c("PUT"),
      trace: c("TRACE")
    },
    trace: n("TRACE")
  };
}, Ue = (t) => ({
  ...t,
  ...fe.getConfig()
}), U = Oe(Ue(Z({
  baseUrl: "https://localhost:44313"
})));
class q {
  static ping(e) {
    return (e?.client ?? U).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/membercomments/api/v1/ping",
      ...e
    });
  }
  static whatsMyName(e) {
    return (e?.client ?? U).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/membercomments/api/v1/whatsMyName",
      ...e
    });
  }
  static whatsTheTimeMrWolf(e) {
    return (e?.client ?? U).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/membercomments/api/v1/whatsTheTimeMrWolf",
      ...e
    });
  }
  static whoAmI(e) {
    return (e?.client ?? U).get({
      security: [
        {
          scheme: "bearer",
          type: "http"
        }
      ],
      url: "/umbraco/membercomments/api/v1/whoAmI",
      ...e
    });
  }
}
var We = Object.defineProperty, je = Object.getOwnPropertyDescriptor, ee = (t) => {
  throw TypeError(t);
}, $ = (t, e, r, s) => {
  for (var a = s > 1 ? void 0 : s ? je(e, r) : e, u = t.length - 1, o; u >= 0; u--)
    (o = t[u]) && (a = (s ? o(e, r, a) : o(a)) || a);
  return s && a && We(e, r, a), a;
}, te = (t, e, r) => e.has(t) || ee("Cannot " + r), T = (t, e, r) => (te(t, e, "read from private field"), r ? r.call(t) : e.get(t)), W = (t, e, r) => e.has(t) ? ee("Cannot add the same private member more than once") : e instanceof WeakSet ? e.add(t) : e.set(t, r), Ne = (t, e, r, s) => (te(t, e, "write to private field"), e.set(t, r), r), k, D, M, P;
let v = class extends ue(ne) {
  constructor() {
    super(), this._yourName = "Press the button!", W(this, k), W(this, D, async (t) => {
      const e = t.target;
      e.state = "waiting";
      const { data: r, error: s } = await q.whoAmI();
      if (s) {
        e.state = "failed", console.error(s);
        return;
      }
      r !== void 0 && (this._serverUserData = r, e.state = "success"), T(this, k) && T(this, k).peek("warning", {
        data: {
          headline: `You are ${this._serverUserData?.name}`,
          message: `Your email is ${this._serverUserData?.email}`
        }
      });
    }), W(this, M, async (t) => {
      const e = t.target;
      e.state = "waiting";
      const { data: r, error: s } = await q.whatsTheTimeMrWolf();
      if (s) {
        e.state = "failed", console.error(s);
        return;
      }
      r !== void 0 && (this._timeFromMrWolf = new Date(r), e.state = "success");
    }), W(this, P, async (t) => {
      const e = t.target;
      e.state = "waiting";
      const { data: r, error: s } = await q.whatsMyName();
      if (s) {
        e.state = "failed", console.error(s);
        return;
      }
      this._yourName = r, e.state = "success";
    }), this.consumeContext(le, (t) => {
      Ne(this, k, t);
    }), this.consumeContext(de, (t) => {
      this.observe(
        t?.currentUser,
        (e) => {
          this._contextCurrentUser = e;
        },
        "_contextCurrentUser"
      );
    });
  }
  render() {
    return V`
      <uui-box headline="Who am I?">
        <div slot="header">[Server]</div>
        <h2>
          <uui-icon name="icon-user"></uui-icon>${this._serverUserData?.email ? this._serverUserData.email : "Press the button!"}
        </h2>
        <ul>
          ${this._serverUserData?.groups.map(
      (t) => V`<li>${t.name}</li>`
    )}
        </ul>
        <uui-button
          color="default"
          look="primary"
          @click="${T(this, D)}"
        >
          Who am I?
        </uui-button>
        <p>
          This endpoint gets your current user from the server and displays your
          email and list of user groups. It also displays a Notification with
          your details.
        </p>
      </uui-box>

      <uui-box headline="What's my Name?">
        <div slot="header">[Server]</div>
        <h2><uui-icon name="icon-user"></uui-icon> ${this._yourName}</h2>
        <uui-button
          color="default"
          look="primary"
          @click="${T(this, P)}"
        >
          Whats my name?
        </uui-button>
        <p>
          This endpoint has a forced delay to show the button 'waiting' state
          for a few seconds before completing the request.
        </p>
      </uui-box>

      <uui-box headline="What's the Time?">
        <div slot="header">[Server]</div>
        <h2>
          <uui-icon name="icon-alarm-clock"></uui-icon> ${this._timeFromMrWolf ? this._timeFromMrWolf.toLocaleString() : "Press the button!"}
        </h2>
        <uui-button
          color="default"
          look="primary"
          @click="${T(this, M)}"
        >
          Whats the time Mr Wolf?
        </uui-button>
        <p>This endpoint gets the current date and time from the server.</p>
      </uui-box>

      <uui-box headline="Who am I?" class="wide">
        <div slot="header">[Context]</div>
        <p>Current user email: <b>${this._contextCurrentUser?.email}</b></p>
        <p>
          This is the JSON object available by consuming the
          'UMB_CURRENT_USER_CONTEXT' context:
        </p>
        <umb-code-block language="json" copy
          >${JSON.stringify(this._contextCurrentUser, null, 2)}</umb-code-block
        >
      </uui-box>
    `;
  }
};
k = /* @__PURE__ */ new WeakMap();
D = /* @__PURE__ */ new WeakMap();
M = /* @__PURE__ */ new WeakMap();
P = /* @__PURE__ */ new WeakMap();
v.styles = [
  oe`
      :host {
        display: grid;
        gap: var(--uui-size-layout-1);
        padding: var(--uui-size-layout-1);
        grid-template-columns: 1fr 1fr 1fr;
      }

      uui-box {
        margin-bottom: var(--uui-size-layout-1);
      }

      h2 {
        margin-top: 0;
      }

      .wide {
        grid-column: span 3;
      }
    `
];
$([
  j()
], v.prototype, "_yourName", 2);
$([
  j()
], v.prototype, "_timeFromMrWolf", 2);
$([
  j()
], v.prototype, "_serverUserData", 2);
$([
  j()
], v.prototype, "_contextCurrentUser", 2);
v = $([
  ce("example-dashboard")
], v);
const Pe = v;
export {
  v as ExampleDashboardElement,
  Pe as default
};
//# sourceMappingURL=dashboard.element-nWl3u5fD.js.map
