uniform sampler2D texture;
uniform float brightness;

void main() {
    vec4 texColor = texture2D(texture, gl_TexCoord[0].xy);
    vec3 brightColor = texColor.rgb + vec3(brightness);
    gl_FragColor = vec4(brightColor, texColor.a);
}
