uniform sampler2D texture;
uniform vec2 tex_offset;

void main() {
    vec2 texCoord = gl_TexCoord[0].xy;
    vec4 result = texture2D(texture, texCoord) * 4.0;
    result += texture2D(texture, texCoord + vec2(tex_offset.x, 0.0)) * 2.0;
    result += texture2D(texture, texCoord - vec2(tex_offset.x, 0.0)) * 2.0;
    result += texture2D(texture, texCoord + vec2(tex_offset.x * 2.0, 0.0)) * 1.0;
    result += texture2D(texture, texCoord - vec2(tex_offset.x * 2.0, 0.0)) * 1.0;
    gl_FragColor = result / 10.0;
}
