const a = [
  {
    name: "Member Comments Entrypoint",
    alias: "MemberComments.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint-BSlTz4-p.js")
  }
], e = [
  {
    name: "Member Comments Dashboard",
    alias: "MemberComments.Dashboard",
    type: "dashboard",
    js: () => import("./dashboard.element-nWl3u5fD.js"),
    meta: {
      label: "Example Dashboard",
      pathname: "example-dashboard"
    },
    conditions: [
      {
        alias: "Umb.Condition.SectionAlias",
        match: "Umb.Section.Content"
      }
    ]
  }
], t = [
  ...a,
  ...e
];
export {
  t as manifests
};
//# sourceMappingURL=member-comments.js.map
