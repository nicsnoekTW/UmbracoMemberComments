export const manifests: Array<UmbExtensionManifest> = [
  {
    name: "Member Comments Entrypoint",
    alias: "MemberComments.Entrypoint",
    type: "backofficeEntryPoint",
    js: () => import("./entrypoint.js"),
  },
];
